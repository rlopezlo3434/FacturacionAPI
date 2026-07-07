using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FacturacionAPI.Models.Enums;
using FacturacionAPI.Migrations;

namespace FacturacionAPI.Services
{
    public class FacturacionService 
    {
        private readonly HttpClient _httpClient;
        private readonly SistemaVentasDbContext _context;
        private CajaService _cajaService;
        //private readonly string _nubefactUrl = "https://api.nubefact.com/api/v1/9a1cbb4b-c878-48d6-8aa5-5996ac27408b";
        //private readonly string _token = "cf09b4ee1e3a42a4a8ef9a83d6a87d8346d94e2386ae41cb81cd3c6e748ad142";

        private const decimal IGV_PERCENT = 18m;
        private const decimal FACTOR_IGV = 1.18m;

        public FacturacionService(HttpClient httpClient, SistemaVentasDbContext context, CajaService cajaService)
        {
            _context = context;
            _httpClient = httpClient;
            _cajaService = cajaService;
        }
        public async Task<IEnumerable<ItemsDto>> GetItemsByEstablishment(int establishmentId)
        {
            return await _context.Items
                .Include(e => e.ProductDefinition)
                .Include(e => e.Stock)
                .Where(e => e.EstablishmentId == establishmentId && e.IsActive == true)
                .Select(e => new ItemsDto
                {
                    Id = e.ProductDefinition.Id,
                    Item = e.ProductDefinition.Item.ToString() == "servicio" ? "Servicio" : "Producto",
                    Value = e.Value,
                    Description = e.ProductDefinition.Description,
                    CreatedAt = e.CreatedAt.ToString("dd/MM/yyyy"),
                    IsActive = e.IsActive,
                    Code = e.ProductDefinition.Code
                })
                .ToListAsync();
        }

        public async Task<object> GetSeries(int establishmentId)
        {
            var establishment = await _context.Establishment
                .FirstOrDefaultAsync(e => e.Id == establishmentId);

            if (establishment == null)
                throw new ApplicationException("No se encontró el establecimiento.");

            // Creamos una lista simple para enviar al frontend
            var series = new List<object>();

            if (!string.IsNullOrEmpty(establishment.SerieFactura))
            {
                series.Add(new
                {
                    tipo = "Factura",
                    serie = establishment.SerieFactura,
                    tipoComprobante = 1 // Según tu mapeo: 1 = FACTURA
                });
            }

            if (!string.IsNullOrEmpty(establishment.SerieBoleta))
            {
                series.Add(new
                {
                    tipo = "Boleta",
                    serie = establishment.SerieBoleta,
                    tipoComprobante = 2 // Según tu mapeo: 2 = BOLETA
                });
            }

            return new
            {
                success = true,
                data = series
            };
        }

        public async Task<object> AnularVentaAsync(int id, int establishmentId)
        {
            var documento = await _context.Ventas.FindAsync(id);

            if (documento == null)
                throw new ApplicationException("Documento no encontrado.");

            if (documento.IsAnnulled)
                throw new ApplicationException("El documento ya fue anulado.");

            // Marcar como anulado en BD de forma inmediata
            documento.IsAnnulled = true;
            _context.Ventas.Update(documento);

            // Registrar pendiente de envío a Nubefact (se enviará a las 3 AM)
            _context.AnulacionDocumento.Add(new AnulacionDocumento
            {
                VentaId = documento.Id,
                CodigoUnico = "",
                Motivo = "BAJA EN EL SISTEMA",
                EnlacePdf = "",
                EnlaceXml = "",
                EnlaceCdr = "",
                EnviadoNubefact = false
            });

            await _context.SaveChangesAsync();

            return new
            {
                mensaje = "Documento anulado en el sistema. Será enviado a Nubefact a las 3 AM.",
                ventaId = documento.Id,
                serie = documento.Serie,
                numero = documento.Numero
            };
        }

        public async Task<object> EnviarAnulacionNubefactAsync(int ventaId, int establishmentId)
        {
            var establishment = await _context.Establishment.FindAsync(establishmentId);
            var documento = await _context.Ventas.FindAsync(ventaId)
                ?? throw new ApplicationException("Documento no encontrado.");
            var anulacionDb = await _context.AnulacionDocumento
                .FirstOrDefaultAsync(a => a.VentaId == ventaId && !a.EnviadoNubefact)
                ?? throw new ApplicationException("No hay anulación pendiente para este documento.");

            var payload = new
            {
                operacion = "generar_anulacion",
                tipo_de_comprobante = documento.TipoComprobante == "BOLETA" ? 2 : 1,
                serie = documento.Serie,
                numero = documento.Numero,
                motivo = "BAJA EN EL SISTEMA",
                codigo_unico = ""
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", establishment?.TokenNubefact);

            var response = await _httpClient.PostAsync(establishment?.urlNubefact, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefact = JsonSerializer.Deserialize<JsonElement>(result);

            anulacionDb.EnlacePdf = nubefact.GetProperty("enlace_del_pdf").GetString() ?? "";
            anulacionDb.EnlaceXml = nubefact.GetProperty("enlace_del_xml").GetString() ?? "";
            anulacionDb.EnlaceCdr = nubefact.GetProperty("enlace_del_cdr").GetString() ?? "";
            anulacionDb.EnviadoNubefact = true;

            await _context.SaveChangesAsync();

            return new { ventaId, enviado = true };
        }

        private string ObtenerCondicionVentaTexto(string? condicion)
        {
            if (condicion != null && condicion.StartsWith("CREDITO_DIAS_"))
            {
                var dias = condicion.Replace("CREDITO_DIAS_", "");
                return $"CRÉDITO A {dias} DÍAS";
            }

            return condicion switch
            {
                "CONTADO" => "CONTADO",
                "CREDITO_CUOTAS" => "CRÉDITO EN CUOTAS",
                _ => "CONTADO"
            };
        }

        public async Task<object> RegistrarVentaAsync(VentaRequest request, int establishmentId)
        {
            var establishment = await _context.Establishment.FindAsync(establishmentId);

            // Si no migra a SUNAT se usa serie interna N001
            var serieEfectiva = request.no_migrar_sunat ? "N001" : request.serie;

            var correlativo = await _context.Ventas
                .Where(v => v.Serie == serieEfectiva && v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;

            var positiveItems = request.items.Where(i => i.value >= 0).ToList();
            var negativeItems = request.items.Where(i => i.value < 0).ToList();

            var items = positiveItems.Select(i =>
            {
                decimal subtotal = i.value;
                decimal valorUnitario = subtotal / i.cantidad;
                decimal igv = Math.Round(subtotal * 0.18m, 2);
                decimal total = Math.Round(subtotal + igv, 2);

                return new
                {
                    unidad_de_medida = "NIU",
                    codigo = string.IsNullOrWhiteSpace(i.code) ? "ITEM" : i.code,
                    descripcion = i.description,
                    cantidad = i.cantidad,
                    valor_unitario = valorUnitario,
                    precio_unitario = Math.Round(valorUnitario * FACTOR_IGV, 2),
                    subtotal,
                    tipo_de_igv = 1,
                    igv,
                    total
                };
            }).ToList();

            decimal descuentoGeneral = negativeItems.Any()
                ? Math.Round(negativeItems.Sum(i => i.value) * -1, 2)
                : 0;

            decimal total = items.Sum(x => (decimal)x.total);
            decimal totalGravada = Math.Round(total / FACTOR_IGV, 2);
            decimal totalIgv = Math.Round(total - totalGravada, 2);
            decimal? detraccionMonto = null;

            if (request.detraccion)
            {
                detraccionMonto = request.detraccion_total ??
                    Math.Round(total * (request.detraccion_porcentaje ?? 12) / 100, 2);
            }
            //Cond_venta = request.tipo_condicion_pago?.Split('_').Take(2).Aggregate((a, b) => $"{a}_{b}"),

            string? marcaFinal = request.items[0].brand;
            string? modeloFinal = request.items[0].model;
            int? anioFinal = request.items[0].anio;
            string? placaFinal = string.IsNullOrWhiteSpace(request.vehiculo_placa) ? request.items[0].placa : request.vehiculo_placa;

            if (!string.IsNullOrWhiteSpace(request.vehiculo_placa))
            {
                var vehiculo = await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Model)
                    .FirstOrDefaultAsync(v => v.Plate == request.vehiculo_placa);

                if (vehiculo != null)
                {
                    marcaFinal = vehiculo.Brand.Name;
                    modeloFinal = vehiculo.Model.Name;
                    anioFinal = vehiculo.Year;
                }
            }

            // 🔥 CREAS LA VENTA (SIN GUARDAR)
            var venta = new Venta
            {
                TipoComprobante = request.tipo_de_comprobante == 2 ? "BOLETA" : "FACTURA",
                Serie = serieEfectiva,
                Numero = nuevoCorrelativo,
                ClienteDocumento = request.cliente_numero,
                ClienteNombre = request.cliente_nombre,
                Direccion = request.direccion,
                TotalGravada = totalGravada,
                TotalIgv = totalIgv,
                Total = Math.Round(total - descuentoGeneral, 2),
                Observaciones = request.observaciones,
                FechaEmision = DateTime.Now,
                MetodoPago = request.metodo_pago,
                EstablishmentId = establishmentId,
                Marca = marcaFinal,
                Modelo = modeloFinal,
                Anio = anioFinal,
                Placa = placaFinal,
                Detraccion = request.detraccion,
                Cond_venta = request.tipo_condicion_pago, // request.tipo_condicion_pago?.Split('_').Take(2).Aggregate((a, b) => $"{a}_{b}"),
                DetraccionPorcentaje = request.detraccion_porcentaje,
                DetraccionMonto = request.detraccion_total,
                DetraccionTipo = request.detraccion_tipo,
                Cuotas = request.tipo_condicion_pago != "CONTADO"
                        ? request.cuotas.Select(c => new Models.Entities.VentaCuota
                        {
                            NumeroCuota = c.Cuota,
                            FechaPago = c.FechaPago,
                            Importe = c.Importe
                        }).ToList()
                        : new List<Models.Entities.VentaCuota>(),
                Detalles = request.items.Select(i => new VentaDetalle
                {
                    Codigo = i.code,
                    Descripcion = i.description,
                    Cantidad = i.cantidad,
                    ValorUnitario = Math.Round(i.value / FACTOR_IGV, 2),
                    PrecioUnitario = i.value,
                    Subtotal = Math.Round((i.value / FACTOR_IGV) * i.cantidad, 2),
                    Igv = Math.Round((i.value / FACTOR_IGV) * 0.18m * i.cantidad, 2),
                    Total = Math.Round(i.value * i.cantidad, 2)
                }).ToList()
            };
            var ventaAlCredito = new List<object>();

            if (request.tipo_condicion_pago != "CONTADO")
            {
                ventaAlCredito = request.cuotas.Select(c => new
                {
                    cuota = c.Cuota,
                    fecha_de_pago = c.FechaPago.ToString("dd-MM-yyyy"),
                    importe = c.Importe
                }).ToList<object>();
            }
            if (request.no_migrar_sunat)
            {
                // 🔥 GUARDADO LOCAL SIN SUNAT (serie N001)
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                var invoiceItemIdsLocal = request.items
                    .Where(i => i.id != null)
                    .Select(i => i.id)
                    .ToList();

                var invoiceItemsLocal = await _context.InvoicesItem
                    .Where(ii => invoiceItemIdsLocal.Contains(ii.Id))
                    .ToListAsync();

                foreach (var item in invoiceItemsLocal)
                    item.Invoiced = true;

                await _context.SaveChangesAsync();

                return new
                {
                    success = true,
                    message = "Venta registrada localmente (no migrada a SUNAT)",
                    ventaId = venta.Id,
                    correlativo = $"{venta.Serie}-{venta.Numero}"
                };
            }

            // 🔹 ENVÍO A NUBEFACT
            // total_igv y total_gravada deben coincidir con la suma de las líneas (sin descontar)
            // el descuento_general reduce solo el total final
            decimal totalConDescuento = Math.Round(total - descuentoGeneral, 2);

            var json = JsonSerializer.Serialize(new
            {
                operacion = "generar_comprobante",
                tipo_de_comprobante = request.tipo_de_comprobante,
                sunat_transaction = 1,
                serie = serieEfectiva,
                descuento_general = descuentoGeneral > 0 ? descuentoGeneral : (decimal?)null,
                total_descuento = descuentoGeneral > 0 ? descuentoGeneral : (decimal?)null,
                numero = nuevoCorrelativo,
                cliente_tipo_de_documento = int.Parse(request.cliente_tipo_documento),
                cliente_numero_de_documento = request.cliente_numero,
                cliente_denominacion = request.cliente_nombre,
                fecha_de_emision = request.fecha_emision?.ToString("dd-MM-yyyy"),
                cliente_direccion = request.direccion,
                total = totalConDescuento,
                total_igv = totalIgv,
                porcentaje_de_igv = IGV_PERCENT,
                total_gravada = totalGravada,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                venta_al_credito = ventaAlCredito,
                moneda = 1,
                items
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", establishment?.TokenNubefact);

            var response = await _httpClient.PostAsync(establishment?.urlNubefact, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefactResp = JsonSerializer.Deserialize<JsonElement>(result);

            bool aceptadaPorSunat = nubefactResp.GetProperty("aceptada_por_sunat").GetBoolean();

            //if (!aceptadaPorSunat)
            //    throw new Exception("SUNAT rechazó el comprobante");

            // 🔥 AQUÍ recién completas datos reales
            venta.CodigoHash = nubefactResp.GetProperty("codigo_hash").GetString();
            venta.EnlacePdf = nubefactResp.GetProperty("enlace_del_pdf").GetString();
            venta.EnlaceXml = nubefactResp.GetProperty("enlace_del_xml").GetString();
            venta.EnlaceCdr = nubefactResp.GetProperty("enlace_del_cdr").GetString();

            // 🔥 GUARDAS UNA SOLA VEZ
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            var invoiceItemIds = request.items
                .Where(i => i.id != null)
                .Select(i => i.id)
                .ToList();

            var invoiceItems = await _context.InvoicesItem
                .Where(ii => invoiceItemIds.Contains(ii.Id))
                .ToListAsync();

            foreach (var item in invoiceItems)
            {
                item.Invoiced = true;
            }

            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Venta registrada correctamente en SUNAT",
                ventaId = venta.Id,
                correlativo = $"{venta.Serie}-{venta.Numero}"
            };
        }

        public async Task<object> RegistrarVentaPruebasAsync(VentaRequest request, int establishmentId)
        {
            var establishment = await _context.Establishment.FindAsync(establishmentId);

            var items = request.items.Select(i =>
            {
                decimal subtotal = i.value; // 🔥 ya es total
                decimal valorUnitario = subtotal / i.cantidad;
                decimal precioConIgv = Math.Round(valorUnitario * FACTOR_IGV, 2);
                decimal igv = Math.Round(subtotal * 0.18m, 2);
                decimal total = Math.Round(subtotal + igv, 2);

                return new
                {
                    unidad_de_medida = "NIU",
                    codigo = i.code,
                    descripcion = i.description,
                    cantidad = i.cantidad,
                    valor_unitario = valorUnitario,
                    precio_unitario = precioConIgv,
                    subtotal,
                    tipo_de_igv = 1,
                    igv,
                    total
                };
            }).ToList();

            decimal total = Math.Round(items.Sum(x => (decimal)x.total), 2);
            decimal totalGravada = Math.Round(total / FACTOR_IGV, 2);
            decimal totalIgv = Math.Round(total - totalGravada, 2);
            decimal? detraccionMonto = null;

            // 🔹 Generar correlativo falso para pruebas
            var correlativo = await _context.Ventas
                .Where(v => v.Serie == request.serie && v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;

            if (request.detraccion)
            {
                detraccionMonto = request.detraccion_total ??
                    Math.Round(total * (request.detraccion_porcentaje ?? 12) / 100, 2);
            }

            // 🔹 Crear venta SIN enviar a SUNAT
            var venta = new Venta
            {
                TipoComprobante = request.tipo_de_comprobante == 2 ? "BOLETA" : "FACTURA",
                Serie = request.serie,
                Numero = nuevoCorrelativo,
                ClienteDocumento = request.cliente_numero,
                ClienteNombre = request.cliente_nombre,
                Direccion = request.direccion,
                TotalGravada = totalGravada,
                TotalIgv = totalIgv,
                Total = total,
                Observaciones = request.observaciones,
                CodigoHash = "HASH_FAKE_PARA_PRUEBAS",
                EnlacePdf = "PDF_FAKE_PARA_PRUEBAS",
                EnlaceXml = "XML_FAKE_PARA_PRUEBAS",
                EnlaceCdr = "CDR_FAKE_PARA_PRUEBAS",
                FechaEmision = DateTime.Now,
                MetodoPago = request.metodo_pago,
                EstablishmentId = establishmentId,
                Detalles = request.items.Select(i => new VentaDetalle
                {
                    Codigo = i.code,
                    Descripcion = i.description,
                    Cantidad = i.cantidad,
                    ValorUnitario = Math.Round(i.value / FACTOR_IGV, 2),
                    PrecioUnitario = i.value,
                    Subtotal = Math.Round((i.value / FACTOR_IGV) * i.cantidad, 2),
                    Igv = Math.Round((i.value / FACTOR_IGV) * 0.18m * i.cantidad, 2),
                    Total = Math.Round(i.value * i.cantidad, 2)
                }).ToList()
            };

            _context.Ventas.Add(venta);

            var invoiceItemIds = request.items
                .Where(i => i.id != null)
                .Select(i => i.id)
                .ToList();

            var invoiceItems = await _context.InvoicesItem
                .Where(ii => invoiceItemIds.Contains(ii.Id))
                .ToListAsync();

            foreach (var item in invoiceItems)
            {
                item.Invoiced = true;
            }


            await _context.SaveChangesAsync();
           
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar en BD: " + ex.InnerException?.Message ?? ex.Message);
            }

            return new
            {
                success = true,
                message = "Venta registrada en modo de pruebas (NO enviada a SUNAT)",
                ventaId = venta.Id,
                correlativo = $"{venta.Serie}-{venta.Numero}",
                total,
                totalIgv,
                totalGravada
            };
        }


        public async Task<object> GenerarNotaCreditoAsync(int ventaId, int tipoNotaCredito = 1, string? motivo = null)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Establishment)
                .FirstOrDefaultAsync(v => v.Id == ventaId)
                ?? throw new ApplicationException("Venta no encontrada.");

            if (venta.IsAnnulled)
                throw new ApplicationException("La venta ya está anulada.");

            var establishment = venta.Establishment;

            // Derivar serie NC: F001 → FC01, B001 → BC01
            bool esFactura = venta.TipoComprobante.ToUpper() == "FACTURA";
            int tipoOriginal = esFactura ? 1 : 2;

            string serieOrigen = esFactura
                ? (establishment.SerieFactura ?? venta.Serie)
                : (establishment.SerieBoleta ?? venta.Serie);

            // Convención peruana: FC01 para NC de factura, BC01 para NC de boleta
            string serieNC = esFactura ? "FC02" : "BC02";

            // Obtener correlativo para la serie NC
            var correlativo = await _context.Ventas
                .Where(v => v.Serie == serieNC && v.EstablishmentId == venta.EstablishmentId)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;

            // Un único ítem resumen con el monto exacto del comprobante original.
            decimal totalGravadaNC = venta.TotalGravada;
            decimal totalIgvNC = venta.TotalIgv;
            decimal totalNC = venta.Total;

            // Nubefact exige precio_unitario = valor_unitario * 1.18 estrictamente.
            // Si la venta original tenía descuento, el total no cierra con ese cálculo,
            // por eso se envía descuento_general para que el header siga siendo correcto.
            decimal precioUnitarioBruto = Math.Round(totalGravadaNC * FACTOR_IGV, 2);
            decimal descuentoGeneral = Math.Round(precioUnitarioBruto - totalNC, 2);

            var items = new[]
            {
                new
                {
                    unidad_de_medida = "NIU",
                    codigo = "NC",
                    descripcion = $"ANULACION DE {venta.TipoComprobante} {venta.Serie}-{venta.Numero:D8}",
                    cantidad = 1m,
                    valor_unitario = totalGravadaNC,
                    precio_unitario = precioUnitarioBruto,
                    subtotal = totalGravadaNC,
                    tipo_de_igv = 1,
                    igv = totalIgvNC,
                    total = precioUnitarioBruto
                }
            };

            var motivoTexto = motivo ?? "ANULACION DE LA OPERACION";

            var payload = new
            {
                operacion = "generar_comprobante",
                tipo_de_comprobante = 3,
                sunat_transaction = 1,
                serie = serieNC,
                numero = nuevoCorrelativo,
                tipo_de_nota_de_credito = tipoNotaCredito,
                motivo_o_sustento_de_la_nota_de_credito = motivoTexto,
                documento_que_se_modifica_tipo = tipoOriginal,
                documento_que_se_modifica_serie = venta.Serie,
                documento_que_se_modifica_numero = venta.Numero,
                cliente_tipo_de_documento = venta.ClienteDocumento.Length == 8 ? 1 : 6,
                cliente_numero_de_documento = venta.ClienteDocumento,
                cliente_denominacion = venta.ClienteNombre,
                cliente_direccion = venta.Direccion,
                fecha_de_emision = DateTime.Now.ToString("dd-MM-yyyy"),
                moneda = 1,
                porcentaje_de_igv = IGV_PERCENT,
                descuento_global = descuentoGeneral > 0 ? descuentoGeneral : (decimal?)null,
                total_descuento = descuentoGeneral > 0 ? descuentoGeneral : (decimal?)null,
                total_gravada = totalGravadaNC,
                total_igv = totalIgvNC,
                total = totalNC,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                items
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", establishment.TokenNubefact);

            var response = await _httpClient.PostAsync(establishment.urlNubefact, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefactResp = JsonSerializer.Deserialize<JsonElement>(result);

            // Guardar la NC como una nueva venta en BD
            var notaCredito = new Venta
            {
                TipoComprobante = "NOTA DE CREDITO",
                Serie = serieNC,
                Numero = nuevoCorrelativo,
                ClienteDocumento = venta.ClienteDocumento,
                ClienteNombre = venta.ClienteNombre,
                Direccion = venta.Direccion,
                TotalGravada = totalGravadaNC,
                TotalIgv = totalIgvNC,
                Total = totalNC,
                Observaciones = $"NC de {venta.TipoComprobante} {venta.Serie}-{venta.Numero}. {motivoTexto}",
                FechaEmision = DateTime.Now,
                MetodoPago = venta.MetodoPago,
                EstablishmentId = venta.EstablishmentId,
                Marca = venta.Marca,
                Modelo = venta.Modelo,
                Anio = venta.Anio,
                Placa = venta.Placa,
                CodigoHash = nubefactResp.TryGetProperty("codigo_hash", out var hash) ? hash.GetString() : null,
                EnlacePdf = nubefactResp.TryGetProperty("enlace_del_pdf", out var pdf) ? pdf.GetString() : null,
                EnlaceXml = nubefactResp.TryGetProperty("enlace_del_xml", out var xml) ? xml.GetString() : null,
                EnlaceCdr = nubefactResp.TryGetProperty("enlace_del_cdr", out var cdr) ? cdr.GetString() : null,
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

            _context.Ventas.Add(notaCredito);

            // Marcar la venta original como anulada
            venta.IsAnnulled = true;
            _context.Ventas.Update(venta);

            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Nota de crédito generada correctamente en SUNAT",
                notaCreditoId = notaCredito.Id,
                correlativo = $"{serieNC}-{nuevoCorrelativo:D8}",
                ventaOriginalId = ventaId,
                enlacePdf = notaCredito.EnlacePdf
            };
        }

        public async Task<VentaDetalleResponseDto?> ObtenerVentaDetalleAsync(int ventaId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Cuotas)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                return null;

            return new VentaDetalleResponseDto
            {
                Id = venta.Id,
                TipoComprobante = venta.TipoComprobante,
                Serie = venta.Serie,
                Numero = venta.Numero,
                Correlativo = $"{venta.Serie}-{venta.Numero}",
                Direccion = venta.Direccion,
                ClienteDocumento = venta.ClienteDocumento,
                ClienteNombre = venta.ClienteNombre,
                Marca = venta.Marca,
                Modelo = venta.Modelo,
                Anio = venta.Anio,
                Placa = venta.Placa,
                FechaEmision = venta.FechaEmision,
                Detraccion = venta.Detraccion,
                DetraccionTipo = venta.DetraccionTipo,
                DetraccionMonto = venta.DetraccionMonto,
                DetraccionPorcentaje = venta.DetraccionPorcentaje,
                Subtotal = venta.TotalGravada,
                Igv = venta.TotalIgv,
                Total = venta.Total,
                Cond_venta = venta.Cond_venta,
                Observaciones = venta.Observaciones,

                Detalles = venta.Detalles.Select(d => new VentaDetalleItemDto
                {
                    Codigo = d.Codigo,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    ValorUnitario = d.ValorUnitario,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Igv = d.Igv,
                    Total = d.Total
                }).ToList(),
                Cuotas = venta.Cond_venta != "CONTADO"
                        ? venta.Cuotas.Select(c => new Models.Entities.VentaCuota
                        {
                            NumeroCuota = c.NumeroCuota,
                            FechaPago = c.FechaPago,
                            Importe = c.Importe
                        }).ToList()
                        : new List<Models.Entities.VentaCuota>(),
                Pdf = venta.EnlacePdf,
                Xml = venta.EnlaceXml,
                Cdr = venta.EnlaceCdr
            };
        }

        public async Task<List<VentaEmpleado>> listVentaEmpleado(int establishmentId)
        {
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;  // noviembre

            var inicioMes = new DateTime(year, month, 1);
            var finMes = inicioMes.AddMonths(1);

            return await _context.ventaEmpleados
                .Include(v => v.Empleado)
                .Include(v => v.productDefinition)
                .Include(v => v.Venta)
                    .ThenInclude(v => v.Detalles)
                .Where(v => v.FechaRegistro >= inicioMes &&
                            v.FechaRegistro < finMes &&
                            v.Venta.EstablishmentId == establishmentId &&
                            v.Venta.IsAnnulled == false)
                .ToListAsync();
        }

        public async Task<object> GetComprobantes(int establishmentId, DateTime inicio, DateTime fin)
        {
            var lista = await _context.Ventas
               .Where(v => v.FechaEmision >= inicio && v.FechaEmision < fin)
               .OrderByDescending(v => v.FechaEmision)
               .Select(v => new
               {
                   v.Id,
                   v.TipoComprobante,
                   Serie = v.Serie,
                   Numero = v.Numero,
                   Total = v.Total,
                   Fecha = v.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                   LinkPdf = v.EnlacePdf, // Campo devuelto por Nubefact (guárdalo al registrar)
                   LinkCdr = v.EnlaceCdr,
                   LinkXml = v.EnlaceXml,
                   Anulado = v.IsAnnulled,
                   ClienteNombre = v.ClienteNombre,
                   ClienteNumero = v.ClienteDocumento
               })
               .ToListAsync();
            return lista;
        }

        public async Task<List<ReporteDiarioDto>> GenerarReporteMensual(int establishmentId, DateTime inicio, DateTime fin)
        {
            var ventas = await _context.Ventas
                                .Include(v => v.Detalles)
                                .Where(v =>
                                    v.EstablishmentId == establishmentId &&
                                    v.FechaEmision >= inicio &&
                                    v.FechaEmision <= fin)
                                .ToListAsync();

            var reporte = ventas.Select(v => new ReporteDiarioDto
            {
                Id = v.Id,
                TipoComprobante = v.TipoComprobante,
                Serie = v.Serie,
                Numero = v.Numero,
                ClienteDocumento = v.ClienteDocumento,
                ClienteNombre = v.ClienteNombre,
                FechaEmision = v.FechaEmision,
                Observaciones = v.Observaciones,
                EstadoSunat = v.IsAnnulled ? "SI" : "NO",
                MetodoPago = v.MetodoPago.ToString(),
                Detalles = v.Detalles.Select(d => new ReporteDetalleDto
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
            }).ToList();

            return reporte;

        }
        public async Task<List<ReporteDiarioDto>> GenerarReporteDiario(int establishmentId, DateTime fecha)
        {

            //var fecha = new DateTime(2025, 11, 23);
            // Traer todas las ventas del día para el establecimiento
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.EstablishmentId == establishmentId && v.FechaEmision >= fecha
        && v.FechaEmision < fecha.AddDays(1))
                .OrderBy(v => v.FechaEmision)
                .ToListAsync();

            // Mapear a DTO
            var reporte = ventas.Select(v => new ReporteDiarioDto
            {
                Id = v.Id,
                TipoComprobante = v.TipoComprobante,
                Serie = v.Serie,
                Numero = v.Numero,
                ClienteDocumento = v.ClienteDocumento,
                ClienteNombre = v.ClienteNombre,
                FechaEmision = v.FechaEmision,
                Observaciones = v.Observaciones,
                EstadoSunat = v.IsAnnulled ? "SI" : "NO",
                MetodoPago = v.MetodoPago.ToString(),
                Detalles = v.Detalles.Select(d => new ReporteDetalleDto
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
            }).ToList();

            return reporte;
        }

        public async Task<(bool Success, string Message, int InvoiceId)>
        CreateInvoiceFromApprovedItemsAsync(int intakeId)
        {
            var realIntakeId = await _context.VehicleIntakes
                .Where(x => x.Correlativo == intakeId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (realIntakeId == 0)
                return (false, "Internamiento no existe.", 0);

            // Buscar o crear la invoice del internamiento
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(x => x.VehicleIntakeId == realIntakeId && x.IsActive);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    VehicleIntakeId = realIntakeId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
            }

            // 1. Eliminar todos los items anteriores de la invoice
            var oldItems = await _context.InvoicesItem
                .Where(x => x.InvoiceId == invoice.Id)
                .ToListAsync();

            _context.InvoicesItem.RemoveRange(oldItems);
            await _context.SaveChangesAsync();

            // 2. Cargar los items aprobados actuales de todos los presupuestos del internamiento
            var approvedItems = await _context.VehicleBudgetItems
                .Where(x =>
                    x.IsApproved &&
                    x.VehicleBudget.VehicleIntakeId == realIntakeId &&
                    x.VehicleBudget.IsActive)
                .ToListAsync();

            if (!approvedItems.Any())
                return (false, "No hay ítems aprobados para este internamiento.", invoice.Id);

            // 3. Insertar los nuevos
            foreach (var item in approvedItems)
            {
                _context.InvoicesItem.Add(new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    VehicleBudgetItemId = item.Id,
                    ItemType = item.ItemType,
                    ProductId = item.ProductId,
                    ServiceMasterId = item.ServiceMasterId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TotalPrice = item.TotalPrice,
                    ServicePackageId = item.ServicePackageId
                });
            }

            invoice.Total = approvedItems.Sum(x => x.TotalPrice);

            await _context.SaveChangesAsync();

            return (true, $"Invoice sincronizada con {approvedItems.Count} ítems aprobados.", invoice.Id);
        }

        public async Task<List<InvoiceSelectableItemDto>>
        GetApprovedItemsForInvoiceAsync()
        {
            var items = await _context.InvoicesItem
                                     .Include(i => i.Product)
                                     .Include(i => i.ServiceMaster)
                                     .Select(i => new InvoiceSelectableItemDto
                                     {
                                         BudgetItemId = i.Id,
                                         IntakeCode = i.VehicleBudgetItem.VehicleBudget.Code,
                                         ClienteNombre = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Client.Names,
                                         ClienteNumero = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Client.DocumentIdentificationNumber,
                                         Brand = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Vehicle.Brand.Name,
                                         Model = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Vehicle.Model.Name,
                                         Anio = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Vehicle.Year,
                                         Placa = i.VehicleBudgetItem.VehicleBudget.VehicleIntake.Vehicle.Plate,
                                         Description =
                                             i.Product != null
                                                 ? i.Product.Name
                                                 : i.ServiceMaster!.Name,
                                         ItemType = (int)i.ItemType,
                                         Quantity = i.Quantity,
                                         Discount = i.Discount,
                                         UnitPrice = i.UnitPrice,
                                         SubTotal = i.TotalPrice,
                                         Selected = false,
                                         Invoiced = i.Invoiced,
                                         ServicePackageId = i.ServicePackageId,
                                         Moneda = i.VehicleBudgetItem.VehicleBudget.Moneda
                                     })
                                     .ToListAsync();
            return items;
        
        }


    }
}
