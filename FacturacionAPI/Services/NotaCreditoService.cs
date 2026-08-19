using FacturacionAPI.Data;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FacturacionAPI.Services
{
    public class NotaCreditoService
    {
        private readonly HttpClient _httpClient;
        private readonly SistemaVentasDbContext _context;

        private const decimal IGV_PERCENT = 18m;
        private const decimal FACTOR_IGV = 1.18m;

        public NotaCreditoService(HttpClient httpClient, SistemaVentasDbContext context)
        {
            _context = context;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Genera una Nota de Crédito de tipo "Anulación de la operación" (tipo 1) que referencia
        /// la venta original e indica el documento que la reemplaza.
        /// </summary>
        /// <param name="ventaOriginalId">ID de la venta (boleta o factura) que se va a anular.</param>
        /// <param name="reemplazadoPor">
        ///   Serie y número del nuevo documento, por ej: "F002-15" o "B002-10".
        ///   El prefijo (F/B) determina el texto del motivo.
        /// </param>
        public async Task<object> GenerarNotaCreditoAsync(int ventaOriginalId, string reemplazadoPor)
        {
            var ventaOriginal = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == ventaOriginalId)
                ?? throw new ApplicationException("Venta original no encontrada.");

            if (ventaOriginal.IsAnnulled)
                throw new ApplicationException("La venta original ya fue anulada.");

            var establishment = await _context.Establishment
                .FirstOrDefaultAsync(e => e.Id == ventaOriginal.EstablishmentId)
                ?? throw new ApplicationException("Establecimiento no encontrado.");

            // Determinar si la venta original es Factura o Boleta
            bool esFactura = ventaOriginal.TipoComprobante.ToUpper() == "FACTURA";

            // Tipo de documento que se modifica: 1=Factura, 2=Boleta
            int tipoDocumentoModificado = esFactura ? 1 : 2;

            // Serie de la NC según el tipo del documento original
            string serieNC = esFactura
                ? (establishment.SerieNotaCredito2 ?? throw new ApplicationException("El establecimiento no tiene configurada SerieNotaCredito2 (NC para facturas)."))
                : (establishment.SerieNotaCredito  ?? throw new ApplicationException("El establecimiento no tiene configurada SerieNotaCredito (NC para boletas)."));

            string tipoReemplazo = reemplazadoPor.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "FACTURA" : "BOLETA";
            string motivo = $"SE REMPLAZO POR {tipoReemplazo} {reemplazadoPor.ToUpper()}";

            // Correlativo
            var ultimoNumero = await _context.Ventas
                .Where(v => v.Serie == serieNC && v.EstablishmentId == ventaOriginal.EstablishmentId)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();
            var nuevoCorrelativo = ultimoNumero == 0 ? 1 : ultimoNumero + 1;

            // Totales de la NC (iguales al documento original)
            decimal totalGravadaNC = ventaOriginal.TotalGravada;
            decimal totalIgvNC     = ventaOriginal.TotalIgv;
            decimal totalNC        = ventaOriginal.Total;

            // Items de Nubefact copiados del documento original
            var itemsNC = ventaOriginal.Detalles.Select(d => new
            {
                unidad_de_medida = "NIU",
                codigo           = d.Codigo,
                descripcion      = d.Descripcion,
                cantidad         = d.Cantidad,
                valor_unitario   = d.ValorUnitario,
                precio_unitario  = d.PrecioUnitario,
                subtotal         = d.Subtotal,
                tipo_de_igv      = 1,
                igv              = d.Igv,
                total            = d.Total
            }).ToList();

            // Validar que los totales de los ítems cuadren; si hay diferencia usar descuento_global
            decimal totalItemsNC = Math.Round(itemsNC.Sum(i => (decimal)i.total), 2);
            decimal descuentoGlobal = Math.Round(totalItemsNC - totalNC, 2);

            // Payload Nubefact para Nota de Crédito
            var payload = new Dictionary<string, object>
            {
                ["operacion"]                              = "generar_comprobante",
                ["tipo_de_comprobante"]                    = 3,
                ["serie"]                                  = serieNC,
                ["numero"]                                 = nuevoCorrelativo,
                ["sunat_transaction"]                      = 1,
                ["tipo_de_nota_de_credito"]                = 1,
                ["documento_que_se_modifica_tipo"]         = tipoDocumentoModificado,
                ["documento_que_se_modifica_serie"]        = ventaOriginal.Serie,
                ["documento_que_se_modifica_numero"]       = ventaOriginal.Numero,
                ["cliente_tipo_de_documento"]              = ventaOriginal.ClienteDocumento?.Length == 8 ? 1 : 6,
                ["cliente_numero_de_documento"]            = ventaOriginal.ClienteDocumento ?? "",
                ["cliente_denominacion"]                   = ventaOriginal.ClienteNombre ?? "",
                ["cliente_direccion"]                      = "",
                ["fecha_de_emision"]                       = DateTime.Now.ToString("dd-MM-yyyy"),
                ["moneda"]                                 = 1,
                ["porcentaje_de_igv"]                      = IGV_PERCENT,
                ["total_gravada"]                          = totalGravadaNC,
                ["total_igv"]                              = totalIgvNC,
                ["total"]                                  = totalNC,
                ["enviar_automaticamente_a_la_sunat"]      = true,
                ["enviar_automaticamente_al_cliente"]      = false,
                ["motivo_o_sustento_de_nota"]              = motivo,
                ["items"]                                  = itemsNC
            };

            if (descuentoGlobal > 0)
                payload["descuento_global"] = descuentoGlobal;

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMsg = new HttpRequestMessage(HttpMethod.Post, establishment.urlNubefact) { Content = content };
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Token", establishment.TokenNubefact);

            var response = await _httpClient.SendAsync(requestMsg);
            var result   = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefactResp = JsonSerializer.Deserialize<JsonElement>(result);

            // Guardar NC en Venta
            var ventaNC = new Venta
            {
                TipoComprobante  = "NOTA DE CREDITO",
                Serie            = serieNC,
                Numero           = nuevoCorrelativo,
                ClienteDocumento = ventaOriginal.ClienteDocumento,
                ClienteNombre    = ventaOriginal.ClienteNombre,
                TotalGravada     = totalGravadaNC,
                TotalIgv         = totalIgvNC,
                Total            = totalNC,
                Observaciones    = motivo,
                CodigoHash       = nubefactResp.TryGetProperty("codigo_hash", out var hash) ? hash.GetString() : null,
                EnlacePdf        = nubefactResp.TryGetProperty("enlace_del_pdf", out var pdf)  ? pdf.GetString()  : null,
                FechaEmision     = DateTime.Now,
                MetodoPago       = ventaOriginal.MetodoPago,
                EstablishmentId  = ventaOriginal.EstablishmentId,
                Detalles = ventaOriginal.Detalles.Select(d => new VentaDetalle
                {
                    Codigo         = d.Codigo,
                    Descripcion    = d.Descripcion,
                    Cantidad       = d.Cantidad,
                    ValorUnitario  = d.ValorUnitario,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal       = d.Subtotal,
                    Igv            = d.Igv,
                    Total          = d.Total
                }).ToList()
            };

            _context.Ventas.Add(ventaNC);

            // Marcar la venta original como anulada
            ventaOriginal.IsAnnulled = true;
            _context.Ventas.Update(ventaOriginal);

            await _context.SaveChangesAsync();

            return new
            {
                success        = true,
                message        = "Nota de Crédito generada correctamente",
                notaCreditoId  = ventaNC.Id,
                serie          = serieNC,
                numero         = nuevoCorrelativo,
                motivo,
                respuesta      = nubefactResp
            };
        }

        public async Task<List<object>> ListarNotasCredito(int establishmentId, DateTime desde, DateTime hasta)
        {
            var fin = hasta.Date.AddDays(1).AddTicks(-1);

            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.TipoComprobante == "NOTA DE CREDITO" &&
                    v.FechaEmision >= desde.Date &&
                    v.FechaEmision <= fin)
                .OrderByDescending(v => v.FechaEmision)
                .Select(v => (object)new
                {
                    v.Id,
                    v.TipoComprobante,
                    v.Serie,
                    v.Numero,
                    v.ClienteDocumento,
                    v.ClienteNombre,
                    v.Total,
                    Fecha   = v.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                    LinkPdf = v.EnlacePdf,
                    v.Observaciones
                })
                .ToListAsync();
        }
    }
}
