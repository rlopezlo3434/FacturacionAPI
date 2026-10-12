using FacturacionAPI.Data;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FacturacionAPI.Services
{
    public class NotaCreditoService
    {
        private readonly SistemaVentasDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FacturacionService _facturacionService;

        public NotaCreditoService(SistemaVentasDbContext context, IHttpClientFactory httpClientFactory, FacturacionService facturacionService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _facturacionService = facturacionService;
        }

        /// <summary>
        /// Genera la Nota de Crédito: crea la Venta de inmediato (visible y con PDF propio
        /// disponible al instante, sin esperar la confirmación real de SUNAT) y decide cómo
        /// se envía a Nubefact según el tipo de documento original:
        /// - Factura: se envía a Nubefact en el acto (no espera el job de las 3am).
        /// - Boleta: queda encolada y se envía al día siguiente (SUNAT la recibe de noche),
        ///   procesada por el job diario de las 3am.
        /// En ambos casos el documento ya es visible para el usuario desde el momento en que
        /// se llama este método.
        /// </summary>
        public async Task<(bool Success, string Message, int PendienteId, int? NcVentaId)>
            EncolarNotaCreditoAsync(int ventaOriginalId, string? reemplazadoPor = null)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Establishment)
                .FirstOrDefaultAsync(v => v.Id == ventaOriginalId);

            if (venta == null)
                return (false, "Venta original no encontrada.", 0, null);

            if (venta.IsAnnulled)
                return (false, "La venta ya fue anulada.", 0, null);

            if (venta.EstablishmentId == null)
                return (false, "La venta no tiene establecimiento.", 0, null);

            // Verificar que no exista ya una NC pendiente o enviada para esta venta
            var yaExiste = await _context.DocumentosPendientesEnvio
                .AnyAsync(d =>
                    d.VentaOriginalId == ventaOriginalId &&
                    d.TipoDocumento == "NOTA_CREDITO" &&
                    d.Estado != "ERROR");

            if (yaExiste)
                return (false, "Ya existe una Nota de Crédito pendiente o enviada para esta venta.", 0, null);

            bool esFactura = venta.TipoComprobante.ToUpper().Contains("FACTURA");
            var establishment = venta.Establishment;

            var serieNC = esFactura ? establishment.SerieNotaCredito2 : establishment.SerieNotaCredito;
            if (string.IsNullOrEmpty(serieNC))
                return (false, $"Serie NC no configurada para {(esFactura ? "Factura" : "Boleta")}.", 0, null);

            var ultimoNumero = await _context.Ventas
                .Where(v => v.Serie == serieNC && v.EstablishmentId == establishment.Id)
                .MaxAsync(v => (int?)v.Numero) ?? 0;
            int nuevoNumero = ultimoNumero + 1;

            string motivo = string.IsNullOrWhiteSpace(reemplazadoPor)
                ? "ANULACION DE OPERACION"
                : $"SE REMPLAZO POR {(reemplazadoPor.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "FACTURA" : "BOLETA")} {reemplazadoPor.ToUpper()}";

            // 🔥 La NC se crea (y queda visible) de inmediato, sea cual sea el camino de envío.
            var ncVenta = new Venta
            {
                TipoComprobante = "NOTA DE CREDITO",
                Serie = serieNC,
                Numero = nuevoNumero,
                ClienteDocumento = venta.ClienteDocumento,
                ClienteNombre = venta.ClienteNombre,
                Direccion = venta.Direccion,
                TotalGravada = venta.TotalGravada,
                TotalIgv = venta.TotalIgv,
                Total = venta.Total,
                Moneda = venta.Moneda,
                Marca = venta.Marca,
                Modelo = venta.Modelo,
                Anio = venta.Anio,
                Placa = venta.Placa,
                FechaEmision = DateTime.Now,
                Observaciones = motivo,
                DocumentoRefSerie = venta.Serie,
                DocumentoRefNumero = venta.Numero,
                DocumentoRefTipo = venta.TipoComprobante,
                DocumentoRefFecha = venta.FechaEmision,
                EstablishmentId = establishment.Id,
                Cond_venta = "CONTADO",
                Detalles = venta.Detalles.Select(d => new VentaDetalle
                {
                    Codigo = d.Codigo,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    ValorUnitario = d.ValorUnitario,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Igv = d.Igv,
                    Total = d.Total
                }).ToList()
            };

            _context.Ventas.Add(ncVenta);

            // Boleta: se envía mañana (SUNAT la recibe de noche). Factura: intento inmediato.
            var fechaProgramada = esFactura
                ? DateTime.Now
                : DateTime.Today.AddDays(1).AddHours(10);

            var pendiente = new DocumentoPendienteEnvio
            {
                TipoDocumento = "NOTA_CREDITO",
                VentaOriginalId = ventaOriginalId,
                ReemplazadoPor = reemplazadoPor,
                FechaProgramada = fechaProgramada,
                Estado = "PENDIENTE",
                EstablishmentId = establishment.Id,
                CreatedAt = DateTime.Now
            };

            _context.DocumentosPendientesEnvio.Add(pendiente);

            // Marcar la venta original como anulada desde ya
            venta.IsAnnulled = true;

            await _context.SaveChangesAsync();

            // Vincular el pendiente con la NC recién creada, para que el envío (inmediato o
            // vía job) actualice esta misma Venta en vez de crear una nueva.
            pendiente.NcVentaId = ncVenta.Id;
            await _context.SaveChangesAsync();

            if (esFactura)
            {
                var (ok, error, _) = await EnviarNotaCreditoANubefactAsync(pendiente);

                pendiente.Intentos++;
                if (ok)
                {
                    pendiente.Estado = "ENVIADO";
                    pendiente.EnviadoAt = DateTime.Now;
                }
                else
                {
                    pendiente.Estado = pendiente.Intentos >= 3 ? "ERROR" : "PENDIENTE";
                    pendiente.MensajeError = error;
                }
                await _context.SaveChangesAsync();
            }

            return (true,
                $"Nota de Crédito {ncVenta.Serie}-{ncVenta.Numero} generada correctamente.",
                pendiente.Id,
                ncVenta.Id);
        }

        /// <summary>
        /// Encola una Anulación para enviarla a Nubefact.
        /// Mismo criterio: boleta → día siguiente, factura → inmediato.
        /// </summary>
        public async Task<(bool Success, string Message, int PendienteId)>
            EncolarAnulacionAsync(int ventaOriginalId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Establishment)
                .FirstOrDefaultAsync(v => v.Id == ventaOriginalId);

            if (venta == null)
                return (false, "Venta no encontrada.", 0);

            if (venta.IsAnnulled)
                return (false, "La venta ya fue anulada.", 0);

            if (venta.EstablishmentId == null)
                return (false, "La venta no tiene establecimiento.", 0);

            bool esFactura = venta.TipoComprobante.ToUpper().Contains("FACTURA");

            var fechaProgramada = esFactura
                ? DateTime.Now
                : DateTime.Today.AddDays(1).AddHours(10);

            var yaExiste = await _context.DocumentosPendientesEnvio
                .AnyAsync(d =>
                    d.VentaOriginalId == ventaOriginalId &&
                    d.TipoDocumento == "ANULACION" &&
                    d.Estado != "ERROR");

            if (yaExiste)
                return (false, "Ya existe una anulación pendiente o enviada para esta venta.", 0);

            var pendiente = new DocumentoPendienteEnvio
            {
                TipoDocumento = "ANULACION",
                VentaOriginalId = ventaOriginalId,
                FechaProgramada = fechaProgramada,
                Estado = "PENDIENTE",
                EstablishmentId = venta.EstablishmentId.Value,
                CreatedAt = DateTime.Now
            };

            _context.DocumentosPendientesEnvio.Add(pendiente);
            venta.IsAnnulled = true;

            await _context.SaveChangesAsync();

            string cuando = esFactura
                ? "inmediatamente"
                : $"mañana {fechaProgramada:dd/MM/yyyy HH:mm}";

            return (true, $"Anulación encolada. Se enviará a Nubefact {cuando}.", pendiente.Id);
        }

        public async Task<List<DocumentoPendienteEnvio>> ListarPendientesAsync(int establishmentId)
        {
            return await _context.DocumentosPendientesEnvio
                .Include(d => d.VentaOriginal)
                .Where(d => d.EstablishmentId == establishmentId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<object>> ListarNotasCredito(int establishmentId, DateTime desde, DateTime hasta)
        {
            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.TipoComprobante == "NOTA DE CREDITO" &&
                    v.FechaEmision >= desde &&
                    v.FechaEmision <= hasta.AddDays(1))
                .OrderByDescending(v => v.FechaEmision)
                .Select(v => (object)new
                {
                    v.Id,
                    v.Serie,
                    v.Numero,
                    v.ClienteNombre,
                    v.ClienteDocumento,
                    v.Total,
                    v.FechaEmision,
                    v.Observaciones,
                    v.EnlacePdf
                })
                .ToListAsync();
        }

        // ─── Envío real a Nubefact (llamado por EnvioDocumentosPendientesService) ───

        internal async Task<(bool Success, string? Error, int? NcVentaId)>
            EnviarNotaCreditoANubefactAsync(DocumentoPendienteEnvio pendiente)
        {
            var ventaOriginal = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Establishment)
                .FirstOrDefaultAsync(v => v.Id == pendiente.VentaOriginalId);

            if (ventaOriginal == null)
                return (false, "Venta original no encontrada.", null);

            var establishment = ventaOriginal.Establishment;
            bool esFactura = ventaOriginal.TipoComprobante.ToUpper().Contains("FACTURA");

            // La NC ya fue creada (y es visible) desde que se encoló. Reutilizamos su
            // serie/número reservados en vez de recalcularlos, para no duplicar el comprobante.
            Venta? ncVentaExistente = pendiente.NcVentaId.HasValue
                ? await _context.Ventas.FirstOrDefaultAsync(v => v.Id == pendiente.NcVentaId.Value)
                : null;

            string serieNC;
            int nuevoNumero;

            if (ncVentaExistente != null)
            {
                serieNC = ncVentaExistente.Serie;
                nuevoNumero = ncVentaExistente.Numero;
            }
            else
            {
                // Compatibilidad con pendientes encolados antes de este cambio (sin NC pre-creada)
                serieNC = esFactura ? establishment.SerieNotaCredito2 : establishment.SerieNotaCredito;

                if (string.IsNullOrEmpty(serieNC))
                    return (false, $"Serie NC no configurada para {(esFactura ? "Factura" : "Boleta")}.", null);

                var ultimoNumero = await _context.Ventas
                    .Where(v => v.Serie == serieNC && v.EstablishmentId == establishment.Id)
                    .MaxAsync(v => (int?)v.Numero) ?? 0;

                nuevoNumero = ultimoNumero + 1;
            }

            string reemplazadoPor = pendiente.ReemplazadoPor ?? "";
            string motivo = string.IsNullOrWhiteSpace(reemplazadoPor)
                ? "ANULACION DE OPERACION"
                : $"SE REMPLAZO POR {(reemplazadoPor.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "FACTURA" : "BOLETA")} {reemplazadoPor.ToUpper()}";

            // Usar un ítem sintético con los totales exactos del header para evitar
            // diferencias de redondeo entre VentaDetalles y lo que Nubefact tiene del comprobante original
            decimal totalGravada = ventaOriginal.TotalGravada;
            decimal totalIgv    = ventaOriginal.TotalIgv;
            decimal totalNC     = ventaOriginal.Total;

            var itemsNC = new[]
            {
                new
                {
                    unidad_de_medida = "NIU",
                    codigo = "NC",
                    descripcion = $"ANULACIÓN {ventaOriginal.Serie}-{ventaOriginal.Numero}",
                    cantidad = 1,
                    valor_unitario = totalGravada,
                    precio_unitario = totalNC,
                    subtotal = totalGravada,
                    tipo_de_igv = 1,
                    igv = totalIgv,
                    total = totalNC,
                    anticipo_regularizacion = false
                }
            };

            // Inferir tipo de documento del cliente por largo: 8=DNI, 11=RUC, 9/12=Carnet de Extranjería
            int tipoDocCliente = FacturacionService.DeterminarTipoDocumento(ventaOriginal.ClienteDocumento);

            decimal? tipoCambio = ventaOriginal.Moneda == "USD"
                ? await _facturacionService.GetTipoCambioSunatAsync()
                : (decimal?)null;

            var payload = new
            {
                operacion = "generar_comprobante",
                tipo_de_comprobante = 3,          // Nota de Crédito siempre es 3
                serie = serieNC,
                numero = nuevoNumero,
                sunat_transaction = 1,
                tipo_de_nota_de_credito = 1,      // 1 = Anulación de la operación
                motivo,
                codigo_unico = Guid.NewGuid().ToString(),
                fecha_de_emision = DateTime.Now.ToString("dd-MM-yyyy"),
                cliente_tipo_de_documento = tipoDocCliente,
                cliente_numero_de_documento = ventaOriginal.ClienteDocumento,
                cliente_denominacion = ventaOriginal.ClienteNombre,
                cliente_direccion = ventaOriginal.Direccion ?? "",
                documento_que_se_modifica_tipo = esFactura ? 1 : 2,
                documento_que_se_modifica_serie = ventaOriginal.Serie,
                documento_que_se_modifica_numero = ventaOriginal.Numero,
                total_gravada = totalGravada,
                total_igv = totalIgv,
                total = totalNC,
                porcentaje_de_igv = 18.00,
                cancelado = true,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente_por_correo = false,
                moneda = ventaOriginal.Moneda == "USD" ? 2 : 1, // 1 = Soles, 2 = Dólares Americanos (Nubefact)
                tipo_de_cambio = tipoCambio,
                items = itemsNC
            };

            var (ok, error, ncVentaId) = await EnviarPayloadAsync(
                establishment, payload, serieNC, nuevoNumero, motivo, ventaOriginal,
                totalGravada, totalIgv, totalNC, ncVentaExistente);

            return (ok, error, ncVentaId);
        }

        internal async Task<(bool Success, string? Error)>
            EnviarAnulacionANubefactAsync(DocumentoPendienteEnvio pendiente)
        {
            var ventaOriginal = await _context.Ventas
                .Include(v => v.Establishment)
                .FirstOrDefaultAsync(v => v.Id == pendiente.VentaOriginalId);

            if (ventaOriginal == null)
                return (false, "Venta original no encontrada.");

            var establishment = ventaOriginal.Establishment;

            // Nubefact anulación usa operacion = "anular_comprobante"
            var payload = new
            {
                operacion = "anular_comprobante",
                tipo_de_comprobante = ventaOriginal.TipoComprobante.ToUpper().Contains("FACTURA") ? 1 : 2,
                serie = ventaOriginal.Serie,
                numero = ventaOriginal.Numero,
                codigo_unico = Guid.NewGuid().ToString(),
                motivo = "ANULACION DE COMPROBANTE"
            };

            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Token token=\"{establishment.TokenNubefact}\"");

            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try { response = await http.PostAsync(establishment.urlNubefact, content); }
            catch (Exception ex) { return (false, ex.Message); }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, body);
            }

            return (true, null);
        }

        private async Task<(bool Success, string? Error, int? NcVentaId)> EnviarPayloadAsync(
            Establishment establishment,
            object payload,
            string serieNC,
            int nuevoNumero,
            string motivo,
            Venta ventaOriginal,
            decimal totalGravada,
            decimal totalIgv,
            decimal totalNC,
            Venta? ncVentaExistente = null)
        {
            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Token token=\"{establishment.TokenNubefact}\"");

            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try { response = await http.PostAsync(establishment.urlNubefact, content); }
            catch (Exception ex) { return (false, ex.Message, null); }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, responseBody, null);

            string? enlacePdf = null, enlaceXml = null, enlaceCdr = null, codigoHash = null;
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                enlacePdf = root.TryGetProperty("enlace_del_pdf", out var pdf) ? pdf.GetString() : null;
                enlaceXml = root.TryGetProperty("enlace_del_xml", out var xml) ? xml.GetString() : null;
                enlaceCdr = root.TryGetProperty("enlace_del_cdr", out var cdr) ? cdr.GetString() : null;
                codigoHash = root.TryGetProperty("codigo_hash", out var hash) ? hash.GetString() : null;
            }
            catch { }

            if (ncVentaExistente != null)
            {
                // 🔥 La NC ya existía (creada al encolar) y ya es visible para el usuario.
                // Solo la completamos con la confirmación real de SUNAT, sin duplicarla.
                ncVentaExistente.CodigoHash = codigoHash;
                ncVentaExistente.EnlacePdf = enlacePdf;
                ncVentaExistente.EnlaceXml = enlaceXml;
                ncVentaExistente.EnlaceCdr = enlaceCdr;

                await _context.SaveChangesAsync();

                return (true, null, ncVentaExistente.Id);
            }

            // Compatibilidad con pendientes encolados antes de este cambio (sin NC pre-creada)
            var ncVenta = new Venta
            {
                TipoComprobante = "NOTA DE CREDITO",
                Serie = serieNC,
                Numero = nuevoNumero,
                ClienteDocumento = ventaOriginal.ClienteDocumento,
                ClienteNombre = ventaOriginal.ClienteNombre,
                Direccion = ventaOriginal.Direccion,
                TotalGravada = totalGravada,
                TotalIgv = totalIgv,
                Total = totalNC,
                Moneda = ventaOriginal.Moneda,
                Marca = ventaOriginal.Marca,
                Modelo = ventaOriginal.Modelo,
                Anio = ventaOriginal.Anio,
                Placa = ventaOriginal.Placa,
                FechaEmision = DateTime.Now,
                Observaciones = motivo,
                DocumentoRefSerie = ventaOriginal.Serie,
                DocumentoRefNumero = ventaOriginal.Numero,
                DocumentoRefTipo = ventaOriginal.TipoComprobante,
                DocumentoRefFecha = ventaOriginal.FechaEmision,
                CodigoHash = codigoHash,
                EnlacePdf = enlacePdf,
                EnlaceXml = enlaceXml,
                EnlaceCdr = enlaceCdr,
                EstablishmentId = establishment.Id,
                Cond_venta = "CONTADO",
                Detalles = ventaOriginal.Detalles.Select(d => new VentaDetalle
                {
                    Codigo = d.Codigo,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    ValorUnitario = d.ValorUnitario,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Igv = d.Igv,
                    Total = d.Total
                }).ToList()
            };

            _context.Ventas.Add(ncVenta);
            await _context.SaveChangesAsync();

            return (true, null, ncVenta.Id);
        }
    }
}
