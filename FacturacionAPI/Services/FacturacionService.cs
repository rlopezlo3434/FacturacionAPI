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
            var establishment = await _context.Establishment.FindAsync(establishmentId);

            var documento = await _context.Ventas.FindAsync(id);

            var anulacion = new
            {
                operacion = "generar_anulacion",
                tipo_de_comprobante = documento.TipoComprobante == "BOLETA" ? 2 : 1, 
                serie = documento.Serie,
                numero = documento.Numero,
                motivo = "BAJA EN EL SISTEMA",
                codigo_unico = ""
            };

            var json = JsonSerializer.Serialize(anulacion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", establishment?.TokenNubefact);

            // 🔹 Llamada a Nubefact
            var response = await _httpClient.PostAsync(establishment?.urlNubefact, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefact = JsonSerializer.Deserialize<JsonElement>(result);

            // Leer campos específicos
            string enlacePdf = nubefact.GetProperty("enlace_del_pdf").GetString();
            string enlaceXml = nubefact.GetProperty("enlace_del_xml").GetString();
            string enlaceCdr = nubefact.GetProperty("enlace_del_cdr").GetString();

            // Guardar en BD
            var anulacionDb = new AnulacionDocumento
            {
                VentaId = documento.Id,
                CodigoUnico = "",
                Motivo = "ERROR DEL SISTEMA",
                EnlacePdf = enlacePdf,
                EnlaceXml = enlaceXml,
                EnlaceCdr = enlaceCdr
            };

            _context.AnulacionDocumento.Add(anulacionDb);

            // 🔥 Aquí actualizamos la venta como anulada
            documento.IsAnnulled = true;
            _context.Ventas.Update(documento);

            await _context.SaveChangesAsync();

            return new
            {
                mensaje = "Documento anulado correctamente",
                pdf = enlacePdf,
                xml = enlaceXml,
                cdr = enlaceCdr
            };

        }
        public async Task<object> RegistrarVentaAsync(VentaRequest request, int establishmentId)
        {
            var establishment = await _context.Establishment.FindAsync(establishmentId);

            // 🔹 Construir items
            var items = request.items.Select(i =>
            {
                // ✅ AHORA VIENE SIN IGV
                decimal valorSinIgv = i.value;
                decimal precioConIgv =
                    Math.Round(valorSinIgv * FACTOR_IGV, 2);
                decimal subtotal =
                    Math.Round(valorSinIgv * i.cantidad, 2);
                decimal igv =
                    Math.Round(subtotal * IGV_PERCENT / 100, 2);
                decimal total =
                    Math.Round(subtotal + igv, 2);

                return new
                {
                    unidad_de_medida = "NIU",
                    codigo = i.code,
                    descripcion = i.description,
                    cantidad = i.cantidad,

                    // ✅ SUNAT
                    valor_unitario = valorSinIgv,
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

            var correlativo = await _context.Ventas
                                    .Where(v => v.Serie == request.serie && v.EstablishmentId == establishmentId)
                                    .OrderByDescending(v => v.Numero)
                                    .Select(v => v.Numero)
                                    .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;

            var comprobante = new
            {
                operacion = "generar_comprobante",
                tipo_de_comprobante = request.tipo_de_comprobante,
                serie = request.serie,
                numero = nuevoCorrelativo,
                sunat_transaction = 1,
                cliente_tipo_de_documento = request.cliente_tipo_documento,
                cliente_numero_de_documento = request.cliente_numero,
                cliente_denominacion = request.cliente_nombre,
                cliente_direccion = "",
                fecha_de_emision = request.fecha_emision?.ToString("dd-MM-yyyy"),
                moneda = 1,
                porcentaje_de_igv = IGV_PERCENT,
                total_gravada = totalGravada,
                total_igv = totalIgv,
                total = total,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                observaciones = request.observaciones,
                items
            };

            var json = JsonSerializer.Serialize(comprobante);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", establishment?.TokenNubefact);

            // 🔹 Llamada a Nubefact
            var response = await _httpClient.PostAsync(establishment?.urlNubefact, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefactResp = JsonSerializer.Deserialize<JsonElement>(result);

            bool aceptadaPorSunat = nubefactResp.TryGetProperty("aceptada_por_sunat", out var aceptada) && aceptada.GetBoolean();
            bool aceptadaPorNubefact = nubefactResp.TryGetProperty("aceptada_por_nubefact", out var aceptadaNube) && aceptadaNube.GetBoolean();

            if (!aceptadaPorSunat || aceptadaPorSunat)
            {
                var venta = new Venta
                {
                    TipoComprobante = request.tipo_de_comprobante == 2 ? "BOLETA" : "FACTURA",
                    Serie = request.serie,
                    Numero = nuevoCorrelativo,
                    ClienteDocumento = request.cliente_numero,
                    ClienteNombre = request.cliente_nombre,
                    TotalGravada = totalGravada,
                    TotalIgv = totalIgv,
                    Total = total,
                    Observaciones = request.observaciones,
                    CodigoHash = nubefactResp.GetProperty("codigo_hash").GetString(),
                    EnlacePdf = nubefactResp.GetProperty("enlace_del_pdf").GetString(),
                    EnlaceXml = nubefactResp.GetProperty("enlace_del_xml").GetString(),
                    EnlaceCdr = nubefactResp.GetProperty("enlace_del_cdr").GetString(),
                    FechaEmision = DateTime.Now,
                    MetodoPago = request.metodo_pago.ToString() == "CONTADO" ? request.metodo_pago.Value : MetodoPago.Efectivo,
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
                        Total = Math.Round(i.value * i.cantidad, 2),
                    }).ToList()
                };

                _context.Ventas.Add(venta);

                // 🔹 Actualizar stock solo para productos
                foreach (var i in request.items)
                {
                    // Buscar el Item en la DB
                    var itemEntity = await _context.Items
                        .Include(x => x.ProductDefinition)
                        .Include(x => x.Stock)
                        .FirstOrDefaultAsync(x => x.EstablishmentId == establishmentId && x.ProductDefinition.Code == i.code);

                    if (itemEntity != null && itemEntity.ProductDefinition.Item == ItemEnum.producto)
                    {
                        if (itemEntity.Stock == null)
                        {
                            itemEntity.Stock = new Stock { Quantity = 0 };
                        }

                        // Crear movimiento de salida
                        var movimiento = new StockMovement
                        {
                            ItemId = itemEntity.Id,
                            MovementType = MovementType.Salida,
                            Quantity = i.cantidad,
                            Notes = $"Venta {venta.Serie}-{venta.Numero}"
                        };
                        _context.StockMovement.Add(movimiento);

                        // Actualizar stock actual
                        itemEntity.Stock.Quantity -= i.cantidad;
                        if (itemEntity.Stock.Quantity < 0)
                            itemEntity.Stock.Quantity = 0; // evitar negativos
                    }
                }

                await _context.SaveChangesAsync();

                var products = _context.ProductDefinition.ToList();

                //// 🔹 Guardar empleados asignados por servicio
                //foreach (var item in request.items)
                //{
                //    if (item.empleados != null && item.empleados.Any())
                //    {
                //        var productId = products.Where(x => x.Code == item.code).FirstOrDefault();

                //        foreach (var empleado in item.empleados)
                //        {
                //            _context.ventaEmpleados.Add(new VentaEmpleado
                //            {
                //                VentaId = venta.Id,
                //                EmpleadoId = empleado.id,
                //                ProductDefinitionId = productId.Id,
                //                FechaRegistro = DateTime.Now
                //            });
                //        }
                //    }
                //}

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al guardar en BD: " + ex.InnerException?.Message ?? ex.Message);
                }

                try
                {
                    await _cajaService.RegistrarMovimientoPorVenta(venta.Id);
                }
                catch (Exception ex)
                {
                    // Si quieres que NO falle la venta aunque falle caja, puedes ignorarlo
                    throw new Exception("Venta registrada, pero error al registrar movimiento en caja: " + ex.Message);
                }

            }

            return new
            {
                success = true,
                message = "Venta procesada correctamente",
                respuesta = nubefactResp
            };
        }

        public async Task<object> RegistrarVentaPruebasAsync(VentaRequest request, int establishmentId)
        {
            var establishment = await _context.Establishment.FindAsync(establishmentId);

            var items = request.items.Select(i =>
            {
                // ✅ AHORA VIENE SIN IGV
                decimal valorSinIgv = i.value;
                decimal precioConIgv =
                    Math.Round(valorSinIgv * FACTOR_IGV, 2);
                decimal subtotal =
                    Math.Round(valorSinIgv * i.cantidad, 2);
                decimal igv =
                    Math.Round(subtotal * IGV_PERCENT / 100, 2);
                decimal total =
                    Math.Round(subtotal + igv, 2);

                return new
                {
                    unidad_de_medida = "NIU",
                    codigo = i.code,
                    descripcion = i.description,
                    cantidad = i.cantidad,
                    valor_unitario = valorSinIgv,
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
                TotalGravada = totalGravada,
                TotalIgv = totalIgv,
                Total = total,
                Observaciones = request.observaciones,
                CodigoHash = "HASH_FAKE_PARA_PRUEBAS",
                EnlacePdf = "PDF_FAKE_PARA_PRUEBAS",
                EnlaceXml = "XML_FAKE_PARA_PRUEBAS",
                EnlaceCdr = "CDR_FAKE_PARA_PRUEBAS",
                FechaEmision = DateTime.Now,
                MetodoPago = request.metodo_pago.ToString() == "CONTADO" ? request.metodo_pago.Value : MetodoPago.Efectivo,
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

            // 🔹 Respuesta mock estilo Nubefact
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

        public async Task<object> GetComprobantes(int establishmentId, DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = fecha.Date.AddDays(1);

            var lista = await _context.Ventas
               //.Where(v => v.EstablishmentId == establishmentId && v.FechaEmision >= inicio && v.FechaEmision < fin) // Factura o Boleta
               .Where(v => v.EstablishmentId == establishmentId) // Factura o Boleta
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
            // ✅ items aprobados NO facturados
            var approvedItems = await _context.VehicleBudgetItems
                .Include(x => x.VehicleBudget)
               .Where(x =>
                    x.IsApproved &&
                    !x.IsInvoiced &&
                    x.VehicleBudget.VehicleIntakeId == intakeId &&
                    x.VehicleBudget.IsActive)
                .ToListAsync();

            if (!approvedItems.Any())
                return (false, "No existen nuevos items aprobados para facturar.", 0);

            var invoice = new Invoice
            {
                VehicleIntakeId = intakeId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            decimal total = 0;

            foreach (var item in approvedItems)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    VehicleBudgetItemId = item.Id, // ⭐ CLAVE

                    ItemType = item.ItemType,
                    ProductId = item.ProductId,
                    ServiceMasterId = item.ServiceMasterId,

                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TotalPrice = item.TotalPrice
                });

                item.IsInvoiced = true;

                total += item.TotalPrice;
            }

            invoice.Total = total;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return (true, "Factura generada correctamente.", invoice.Id);
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
                                         Invoiced = i.Invoiced
                                     })
                                     .ToListAsync();
            return items;
        
        }


    }
}
