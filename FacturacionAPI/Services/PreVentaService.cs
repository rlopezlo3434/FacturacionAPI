using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FacturacionAPI.Services
{
    public class PreVentaService
    {
        private readonly SistemaVentasDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly CajaService _cajaService;

        private const decimal IGV_PERCENT = 18m;
        private const decimal FACTOR_IGV = 1.18m;

        public PreVentaService(SistemaVentasDbContext context, HttpClient httpClient, CajaService cajaService)
        {
            _context = context;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _cajaService = cajaService;
        }

        public async Task<object> RegistrarPreVentaAsync(VentaRequest request, int establishmentId)
        {
            decimal total = Math.Round(request.items.Sum(i => Math.Round(i.value * i.cantidad, 2)), 2);
            decimal totalGravada = Math.Round(total / FACTOR_IGV, 2);
            decimal totalIgv = Math.Round(total - totalGravada, 2);

            var venta = new Venta
            {
                TipoComprobante = request.tipo_de_comprobante == 2 ? "BOLETA" : "FACTURA",
                Serie = string.Empty,
                Numero = 0,
                ClienteDocumento = request.cliente_numero,
                ClienteNombre = request.cliente_nombre,
                TotalGravada = totalGravada,
                TotalIgv = totalIgv,
                Total = total,
                Observaciones = request.observaciones,
                CodigoHash = null,
                EnlacePdf = null,
                FechaEmision = request.fecha_emision,
                MetodoPago = request.metodo_pago,
                EstablishmentId = establishmentId,
                EsPreVenta = true,
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
            await _context.SaveChangesAsync();

            // Empleados por servicio
            var products = await _context.ProductDefinition.ToListAsync();
            foreach (var item in request.items)
            {
                if (item.empleados != null && item.empleados.Any())
                {
                    var productDef = products.FirstOrDefault(x => x.Code == item.code);
                    foreach (var empleado in item.empleados)
                    {
                        _context.ventaEmpleados.Add(new VentaEmpleado
                        {
                            VentaId = venta.Id,
                            EmpleadoId = empleado.id,
                            ProductDefinitionId = productDef.Id,
                            FechaRegistro = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Pre-venta registrada correctamente. Pendiente de emisión a SUNAT.",
                preVentaId = venta.Id
            };
        }

        public async Task<object> EmitirPreVentaAsync(int preVentaId, string serie, int establishmentId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == preVentaId && v.EstablishmentId == establishmentId && v.EsPreVenta == true);

            if (venta == null)
                throw new ApplicationException("Pre-venta no encontrada o ya fue emitida.");

            var establishment = await _context.Establishment.FindAsync(establishmentId);

            var correlativo = await _context.Ventas
                .Where(v => v.Serie == serie && v.EstablishmentId == establishmentId && v.EsPreVenta == false)
                .OrderByDescending(v => v.Numero)
                .Select(v => v.Numero)
                .FirstOrDefaultAsync();

            var nuevoCorrelativo = correlativo == 0 ? 1 : correlativo + 1;
            int tipoComprobante = venta.TipoComprobante == "BOLETA" ? 2 : 1;

            var items = venta.Detalles.Select(d => new
            {
                unidad_de_medida = "NIU",
                codigo = d.Codigo,
                descripcion = d.Descripcion,
                cantidad = d.Cantidad,
                valor_unitario = d.ValorUnitario,
                precio_unitario = d.PrecioUnitario,
                subtotal = d.Subtotal,
                tipo_de_igv = 1,
                igv = d.Igv,
                total = d.Total
            }).ToList();

            var comprobante = new
            {
                operacion = "generar_comprobante",
                tipo_de_comprobante = tipoComprobante,
                serie,
                numero = nuevoCorrelativo,
                sunat_transaction = 1,
                cliente_tipo_de_documento = venta.ClienteDocumento.Length == 8 ? 1 : 6,
                cliente_numero_de_documento = venta.ClienteDocumento,
                cliente_denominacion = venta.ClienteNombre,
                cliente_direccion = "",
                fecha_de_emision = venta.FechaEmision.ToString("dd-MM-yyyy"),
                moneda = 1,
                porcentaje_de_igv = IGV_PERCENT,
                total_gravada = venta.TotalGravada,
                total_igv = venta.TotalIgv,
                total = venta.Total,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                observaciones = venta.Observaciones,
                items
            };

            var json = JsonSerializer.Serialize(comprobante);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMsg = new HttpRequestMessage(HttpMethod.Post, establishment?.urlNubefact) { Content = content };
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Token", establishment?.TokenNubefact);
            var response = await _httpClient.SendAsync(requestMsg);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en Nubefact: {result}");

            var nubefactResp = JsonSerializer.Deserialize<JsonElement>(result);

            // Actualizar la venta con los datos de Nubefact
            venta.Serie = serie;
            venta.Numero = nuevoCorrelativo;
            venta.CodigoHash = nubefactResp.GetProperty("codigo_hash").GetString();
            venta.EnlacePdf = nubefactResp.GetProperty("enlace_del_pdf").GetString();
            venta.EsPreVenta = false;

            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();

            await _cajaService.RegistrarMovimientoPorVenta(venta.Id);

            return new
            {
                success = true,
                message = "Pre-venta emitida correctamente a SUNAT.",
                ventaId = venta.Id,
                serie,
                numero = nuevoCorrelativo,
                respuesta = nubefactResp
            };
        }

        public async Task<object> EliminarPreVentaAsync(int preVentaId, int establishmentId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == preVentaId && v.EstablishmentId == establishmentId && v.EsPreVenta == true);

            if (venta == null)
                throw new ApplicationException("Pre-venta no encontrada o ya fue emitida.");

            var empleados = await _context.ventaEmpleados.Where(e => e.VentaId == preVentaId).ToListAsync();
            _context.ventaEmpleados.RemoveRange(empleados);
            _context.ventaDetalles.RemoveRange(venta.Detalles);
            _context.Ventas.Remove(venta);

            await _context.SaveChangesAsync();

            return new { success = true, message = "Pre-venta eliminada correctamente." };
        }

        public async Task<object> GetPreVentasPendientes(int establishmentId)
        {
            var lista = await _context.Ventas
                .Where(v => v.EstablishmentId == establishmentId && v.EsPreVenta == true && v.IsAnnulled == false)
                .OrderByDescending(v => v.FechaEmision)
                .Select(v => new
                {
                    v.Id,
                    v.TipoComprobante,
                    v.ClienteDocumento,
                    v.ClienteNombre,
                    v.Total,
                    Fecha = v.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                    v.MetodoPago,
                    v.Observaciones
                })
                .ToListAsync();

            return lista;
        }
    }
}
