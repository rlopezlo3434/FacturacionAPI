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
            if (!string.IsNullOrWhiteSpace(request.codigoPromocional))
            {
                // Verificar que exista
                var promo = await _context.Promotion
                    .FirstOrDefaultAsync(p =>
                        p.Code == request.codigoPromocional &&
                        p.EstablishmentId == establishmentId);

                if (promo == null)
                    throw new ApplicationException("El código promocional no existe.");

                // Verificar que NO haya sido usado por el cliente
                var yaUsado = await _context.CodigosUtilizados
                    .AnyAsync(c =>
                        c.Code == request.codigoPromocional &&
                        c.Client.DocumentIdentificationNumber == request.cliente_numero);

                if (yaUsado)
                    throw new ApplicationException("Este código promocional ya fue usado por este cliente.");
            }

            var establishment = await _context.Establishment.FindAsync(establishmentId);

            // 🔹 Construir items
            var items = request.items.Select(i =>
            {
                decimal precioConIgv = i.value;
                decimal valorSinIgv = Math.Round(precioConIgv / FACTOR_IGV, 2);
                decimal subtotal = Math.Round(valorSinIgv * i.cantidad, 2);
                decimal igv = Math.Round(subtotal * IGV_PERCENT / 100, 2);
                decimal total = Math.Round(precioConIgv * i.cantidad, 2);

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
                fecha_de_emision = request.fecha_emision.ToString("dd-MM-yyyy"),
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

                // 🔹 Guardar empleados asignados por servicio
                foreach (var item in request.items)
                {
                    if (item.empleados != null && item.empleados.Any())
                    {
                        var productId = products.Where(x => x.Code == item.code).FirstOrDefault();

                        foreach (var empleado in item.empleados)
                        {
                            _context.ventaEmpleados.Add(new VentaEmpleado
                            {
                                VentaId = venta.Id,
                                EmpleadoId = empleado.id,
                                ProductDefinitionId = productId.Id,
                                FechaRegistro = DateTime.Now
                            });
                        }
                    }
                }

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

                    if (!string.IsNullOrWhiteSpace(request.codigoPromocional))
                    {
                        var cliente = await _context.Client
                            .FirstOrDefaultAsync(c =>
                                c.DocumentIdentificationNumber == request.cliente_numero);

                        if (cliente != null)
                        {
                            _context.CodigosUtilizados.Add(new CodigosUtilizados
                            {
                                ClientId = cliente.Id,
                                Code = request.codigoPromocional
                            });

                            await _context.SaveChangesAsync();
                        }
                    }


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

            // 🔹 Construir items igual que en el método real
            var items = request.items.Select(i =>
            {
                decimal precioConIgv = i.value;
                decimal valorSinIgv = Math.Round(precioConIgv / FACTOR_IGV, 2);
                decimal subtotal = Math.Round(valorSinIgv * i.cantidad, 2);
                decimal igv = Math.Round(subtotal * IGV_PERCENT / 100, 2);
                decimal total = Math.Round(precioConIgv * i.cantidad, 2);

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

            // 🔹 Generar correlativo falso para pruebas
            var correlativo = await _context.Ventas
                .Where(v => v.Serie == request.serie && v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;

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

            // 🔹 Actualizar stock
            foreach (var i in request.items)
            {
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

                    var movimiento = new StockMovement
                    {
                        ItemId = itemEntity.Id,
                        MovementType = MovementType.Salida,
                        Quantity = i.cantidad,
                        Notes = $"Venta {venta.Serie}-{venta.Numero}"
                    };

                    _context.StockMovement.Add(movimiento);

                    itemEntity.Stock.Quantity -= i.cantidad;
                    if (itemEntity.Stock.Quantity < 0)
                        itemEntity.Stock.Quantity = 0;
                }
            }

            // 🔹 Guardar Venta y Detalles (YA TIENEN ID REAL)
            await _context.SaveChangesAsync();

            // 🔹 MAPEAR: buscar el ID real de cada detalle recién creado
            var detallesMap = venta.Detalles.ToDictionary(d => d.Codigo, d => d.Id);

            var products = _context.ProductDefinition.ToList();

            // 🔹 Guardar empleados asignados por servicio
            foreach (var item in request.items)
            {
                if (item.empleados != null && item.empleados.Any())
                {
                    int ventaItemId = detallesMap[item.code];

                    var productId = products.Where(x => x.Code == item.code).FirstOrDefault();

                    foreach (var empleado in item.empleados)
                    {
                        _context.ventaEmpleados.Add(new VentaEmpleado
                        {
                            VentaId = venta.Id,
                            EmpleadoId = empleado.id,
                            ProductDefinitionId = productId.Id,
                            FechaRegistro = DateTime.Now
                        });
                    }
                }
            }

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

        //public async Task<List<VentaEmpleado>> listVentaEmpleado(int establishmentId)
        //{
        //    var hoy = DateTime.Today;
        //    var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        //    var finMes = inicioMes.AddMonths(1);

        //    return await _context.ventaEmpleados
        //        .Include(v => v.Empleado)
        //        .Include(v => v.productDefinition)
        //        .Include(v => v.Venta)
        //            .ThenInclude(v => v.Detalles)
        //         .Where(v => v.FechaRegistro >= inicioMes &&
        //            v.FechaRegistro < finMes &&
        //            v.Venta.EstablishmentId == establishmentId && v.Venta.IsAnnulled == false)
        //        .ToListAsync();
        //}

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
               .Where(v => v.EstablishmentId == establishmentId && v.FechaEmision >= inicio && v.FechaEmision < fin) // Factura o Boleta
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
                   Anulado = v.IsAnnulled
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

        public async Task<List<ChildrenClientReporteDto>> GenerarReporteClientes(int establishmentId)
        {
            var result = await _context.ChildrenClient
                    .AsNoTracking()
                    .Where(c => c.Client.EstablishmentId == establishmentId && c.IsActive)
                    .SelectMany(c => c.Client.Numbers.DefaultIfEmpty(), (child, phone) => new ChildrenClientReporteDto
                    {
                        // Cliente
                        ClientFirstName = child.Client.FirstName,
                        ClientLastName = child.Client.LastName,
                        DocumentType = child.Client.DocumentIdentificationType.ToString(),
                        DocumentNumber = child.Client.DocumentIdentificationNumber,
                        Email = child.Client.Email,
                        Gender = child.Client.Gender.ToString(),
                        IsActive = child.Client.IsActive,
                        AcceptsMarketing = child.Client.AcceptsMarketing,

                        PhoneNumber = phone != null ? phone.Number : null,

                        // Hijo
                        ChildFirstName = child.FirstName,
                        ChildLastName = child.LastName,
                        FechaCumpleanios = child.FechaCumpleanios
                    })
                    .ToListAsync();

            return result;
        }

        public async Task<List<ReporteDiarioDto>> GenerarReporteAcumulado(int establishmentId, DateTime fecha)
        {

            var hoy = DateTime.Today;

            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesSiguiente = inicioMes.AddMonths(1);

            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < inicioMesSiguiente
                )
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

        public async Task<List<ReporteDiarioDto>> GenerarReporteDiario(int establishmentId, DateTime fecha)
        {

            var hoy = DateTime.Today;

            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesSiguiente = inicioMes.AddMonths(1);

            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < inicioMesSiguiente
                )
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

    }
}
