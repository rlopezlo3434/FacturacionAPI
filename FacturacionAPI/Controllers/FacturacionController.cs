using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Win32;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturacionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly FacturacionService _facturacionService;
        //private readonly string _nubefactUrl = "https://api.nubefact.com/api/v1/9a1cbb4b-c878-48d6-8aa5-5996ac27408b";
        //private readonly string _token = "cf09b4ee1e3a42a4a8ef9a83d6a87d8346d94e2386ae41cb81cd3c6e748ad142";
        public FacturacionController(FacturacionService facturacionService, HttpClient httpClient)
        {
            _facturacionService = facturacionService;
            _httpClient = httpClient;   
        }

        [Authorize]
        [HttpGet("items/by-establishment")]
        public async Task<IActionResult> GetItemsByEstablishment()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var items = await _facturacionService.GetItemsByEstablishment(establishmentId);
            if (items == null || !items.Any())
                return NotFound(new { Message = "No se encontraron items para este establecimiento" });

            return Ok(items);
        }

        [Authorize]
        [HttpGet("documento/{numero}")]
        public async Task<IActionResult> GetDocumento(string numero, [FromQuery] string tipo)
        {
            var token = "sk_11161.sH5SZifYchdcGOaS5jqP0S1Q6bBhSj0e"; // LOPEZ
            //var token = "sk_11995.J1PfLkwiUFu24TB242NfB8y3sFWkaXCH";
            string url;

            if (tipo == "Ruc")
            {
                url = $"https://api.decolecta.com/v1/sunat/ruc?numero={numero}";
            }
            else
            {
                url = $"https://api.decolecta.com/v1/reniec/dni?numero={numero}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [Authorize]
        [HttpPost("registrar-venta")]
        public async Task<IActionResult> RegistrarVenta([FromBody] VentaRequest request)
        {
            if (request == null || request.items == null || !request.items.Any())
                return BadRequest(new { success = false, message = "La venta no contiene ítems válidos." });

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _facturacionService.RegistrarVentaAsync(request, establishmentId);
                //var resultado = await _facturacionService.RegistrarVentaPruebasAsync(request, establishmentId);

                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success= false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("descargar-pdf")]
        public async Task<IActionResult> DescargarPdf(string url)
        {
            using var http = new HttpClient();

            var data = await http.GetByteArrayAsync(url);

            return File(data, "application/pdf", "comprobante.pdf");
        }

        [Authorize]
        [HttpPost("anular/{id}")]
        public async Task<IActionResult> AnularVenta(int id)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _facturacionService.AnularVentaAsync(id, establishmentId);

                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("pdf/print")]
        public async Task<IActionResult> ObtenerPdf([FromQuery] string url)
        {
            using var http = new HttpClient();
            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return BadRequest("No se pudo descargar el PDF externo.");

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            return File(pdfBytes, "application/pdf");
        }

        [Authorize]
        [HttpGet("series")]
        public async Task<IActionResult> GetSeries()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);
            try
            {
                var resultado = await _facturacionService.GetSeries(establishmentId);
                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detalle = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarComprobantes([FromQuery] DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var comprobantes = await _facturacionService.GetComprobantes(establishmentId, fecha);
            if (comprobantes == null)
                return NotFound(new { Message = "No se encontraron comprobantes para este establecimiento" });

            return Ok(comprobantes);
        }


        //[HttpGet("reporte-productividad")]
        //public async Task<IActionResult> ReporteProductividad()
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

        //    var data = await _facturacionService.listVentaEmpleado(establishmentId);
        //    var colorEmpleadoMap = new Dictionary<int, System.Drawing.Color>();
        //    // 🔹 Obtener empleados y productos ordenados por nombre
        //    var empleados = data.Select(x => x.Empleado).Distinct().OrderBy(e => e.FirstName).ToList();
        //    var productos = data.Select(x => x.productDefinition).Distinct().OrderBy(p => p.Description).ToList();

        //    // 🔹 Rango del mes
        //    var hoy = DateTime.Today;
        //    var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        //    var finMes = inicioMes.AddMonths(1);

        //    //int year = DateTime.Today.Year;
        //    //int month = 11; // noviembre

        //    //var inicioMes = new DateTime(year, month, 1);
        //    //var finMes = inicioMes.AddMonths(1);

        //    using var package = new ExcelPackage();
        //    var ws = package.Workbook.Worksheets.Add("Productividad");

        //    int row = 1;
        //    int col = 1;
        //    var colores = new List<System.Drawing.Color>
        //                        {
        //                            System.Drawing.Color.LightPink,
        //                            System.Drawing.Color.LightGreen,
        //                            System.Drawing.Color.LightBlue,
        //                            System.Drawing.Color.LightSalmon,
        //                            System.Drawing.Color.LightYellow,
        //                            System.Drawing.Color.Plum,
        //                            System.Drawing.Color.Khaki,
        //                            System.Drawing.Color.PaleTurquoise,
        //                            System.Drawing.Color.MistyRose
        //                        };

        //    int colorIndex = 0;
        //    // -------------------------------------
        //    //  ENCABEZADO DE FECHA
        //    // -------------------------------------
        //    ws.Cells[row, col].Value = "FECHA";
        //    ws.Cells[row, col].Style.Font.Bold = true;
        //    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
        //    col++;

        //    // -------------------------------------
        //    //  ENCABEZADOS POR EMPLEADO
        //    // -------------------------------------
        //    foreach (var emp in empleados)
        //    {
        //        int inicioColEmpleado = col;
        //        int columnasPorEmpleado = productos.Count + 1; // +1 por MONTO

        //        // Obtiene color para este empleado
        //        var colorEmpleado = colores[colorIndex % colores.Count];
        //        colorIndex++;
        //        colorEmpleadoMap[emp.Id] = colorEmpleado;

        //        // Mezclar celdas para el nombre
        //        ws.Cells[row, col, row, col + columnasPorEmpleado - 1].Merge = true;
        //        ws.Cells[row, col].Value = emp.FirstName.ToUpper();
        //        ws.Cells[row, col].Style.Font.Bold = true;
        //        ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        // 🔹 Asignar color distinto para cada empleado
        //        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorEmpleado);
        //        col += columnasPorEmpleado;
        //    }

        //    row++;

        //    // -------------------------------------
        //    // SUBENCABEZADOS
        //    // -------------------------------------
        //    col = 2;

        //    foreach (var emp in empleados)
        //    {
        //        foreach (var p in productos)
        //        {
        //            ws.Cells[row, col].Value = p.Description;
        //            ws.Cells[row, col].Style.Font.Bold = true;
        //            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //            // 🔹 Ahora asignamos el mismo color del empleado
        //            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorEmpleadoMap[emp.Id]);

        //            // 🔹 Rotar texto hacia arriba
        //            ws.Cells[row, col].Style.TextRotation = 90;

        //            ws.Column(col).Width = 1; // Para que la columna no sea demasiado ancha
        //            col++;
        //        }

        //        ws.Cells[row, col].Value = "MONTO";
        //        ws.Cells[row, col].Style.Font.Bold = true;
        //        ws.Cells[row, col].Style.TextRotation = 90;

        //        ws.Column(col).Width = 1;
        //        col++;
        //    }

        //    row++;

        //    // -------------------------------------
        //    //  CUERPO: Filas por DÍA DEL MES
        //    // -------------------------------------
        //    for (var fecha = inicioMes; fecha < finMes; fecha = fecha.AddDays(1))
        //    {
        //        col = 1;
        //        ws.Cells[row, col].Value = fecha.ToString("dd/MM/yyyy");
        //        col++;

        //        foreach (var emp in empleados)
        //        {
        //            decimal sumaa = 0;

        //            foreach (var p in productos)
        //            {
        //                // Todos los registros del empleado, producto y día
        //                var registros = data
        //                    .Where(x =>
        //                        x.EmpleadoId == emp.Id &&
        //                        x.ProductDefinitionId == p.Id &&
        //                        x.FechaRegistro.Date == fecha.Date
        //                    )
        //                    .ToList();

        //                // CANTIDAD
        //                int cantidad = registros.Count;
        //                ws.Cells[row, col].Value = cantidad == 0 ? "" : cantidad;
        //                col++;

        //                // MONTO POR PRODUCTO (sumando los detalles correctos)
        //                decimal montoProducto = registros
        //                    .SelectMany(x => x.Venta.Detalles)                   // acceder a detalles
        //                    .Where(d => d.Codigo == p.Code)           // detalle de este producto
        //                    .Sum(d => d.PrecioUnitario);                               // subtotales correctos

        //                sumaa += montoProducto;   // acumulamos
        //            }

        //            ws.Cells[row, col].Value = sumaa == 0 ? "" : sumaa;
        //            col++;
        //        }

        //        row++;
        //    }

        //    // -------------------------------------
        //    //  FILA DE TOTALES DEL MES
        //    // -------------------------------------
        //    ws.Cells[row, 1].Value = "TOTAL MES";
        //    ws.Cells[row, 1].Style.Font.Bold = true;
        //    ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //    ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        //    int colTotal = 2;

        //    foreach (var emp in empleados)
        //    {
        //        decimal sum = 0;
        //        foreach (var p in productos)
        //        {
        //            int totalProductoMes = data.Count(x =>
        //                x.EmpleadoId == emp.Id &&
        //                x.ProductDefinitionId == p.Id &&
        //                x.FechaRegistro >= inicioMes &&
        //                x.FechaRegistro < finMes
        //            );

        //            var registros = data.Where(x =>
        //                x.EmpleadoId == emp.Id &&
        //                x.ProductDefinitionId == p.Id &&
        //                x.FechaRegistro >= inicioMes &&
        //                x.FechaRegistro < finMes
        //            ).ToList();

        //            ws.Cells[row, colTotal].Value = totalProductoMes == 0 ? "" : totalProductoMes;
        //            ws.Cells[row, colTotal].Style.Font.Bold = true;
        //            colTotal++;

        //            decimal montoEmpleadoMes2 = registros
        //                    .SelectMany(x => x.Venta.Detalles)                   // acceder a detalles
        //                    .Where(d => d.Codigo == p.Code)           // detalle de este producto
        //                    .Sum(d => d.PrecioUnitario);

        //            sum += montoEmpleadoMes2;   // acumulamos

        //        }




        //        //// Total monto del empleado en el mes
        //        //decimal montoEmpleadoMes = data
        //        //    .Where(x => x.EmpleadoId == emp.Id &&
        //        //                x.FechaRegistro >= inicioMes &&
        //        //                x.FechaRegistro < finMes)
        //        //    .SelectMany(x => x.Venta.Detalles)
        //        //    .Where(d => d != null)
        //        //    .Sum(d => d.PrecioUnitario);

        //        ws.Cells[row, colTotal].Value = sum == 0 ? "" : sum;
        //        ws.Cells[row, colTotal].Style.Font.Bold = true;
        //        colTotal++;
        //    }

        //    // Ajuste de columnas
        //    ws.Cells[ws.Dimension.Address].AutoFitColumns();

        //    var bytes = package.GetAsByteArray();
        //    var fileName = $"Productividad_{DateTime.Now:yyyyMMdd}.xlsx";

        //    return File(bytes,
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        fileName);
        //}


        [Authorize]
        [HttpGet("reporte-diario")]
        public async Task<IActionResult> ReporteDiario(DateTime fecha)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // ✅ funciona bien hasta EPPlus 7.2.2

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);


            var ventas = await _facturacionService.GenerarReporteDiario(establishmentId, fecha);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reporte Diario");

            int row = 1;

            foreach (var venta in ventas)
            {
                // Cabecera de la venta
                worksheet.Cells[row, 1].Value = "Documento";
                worksheet.Cells[row, 2].Value = "Fecha Emisión";
                worksheet.Cells[row, 3].Value = "Cliente Documento";
                worksheet.Cells[row, 4].Value = "Cliente Nombre";
                worksheet.Cells[row, 5].Value = "Observaciones";

                using (var range = worksheet.Cells[row, 1, row, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                row++;

                worksheet.Cells[row, 1].Value = $"{venta.TipoComprobante}-{venta.Serie}-{venta.Numero}";
                worksheet.Cells[row, 2].Value = venta.FechaEmision.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 3].Value = venta.ClienteDocumento;
                worksheet.Cells[row, 4].Value = venta.ClienteNombre;
                worksheet.Cells[row, 5].Value = venta.Observaciones ?? "";
                row++;

                // Detalles de la venta
                if (venta.Detalles != null && venta.Detalles.Any())
                {
                    worksheet.Cells[row, 1].Value = "Código";
                    worksheet.Cells[row, 2].Value = "Descripción";
                    worksheet.Cells[row, 3].Value = "Cantidad";
                    worksheet.Cells[row, 4].Value = "Valor Unitario";
                    worksheet.Cells[row, 5].Value = "Precio Unitario";
                    worksheet.Cells[row, 6].Value = "Subtotal";
                    worksheet.Cells[row, 7].Value = "IGV";
                    worksheet.Cells[row, 8].Value = "Total";

                    using (var range = worksheet.Cells[row, 1, row, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    row++;

                    foreach (var detalle in venta.Detalles)
                    {
                        worksheet.Cells[row, 1].Value = detalle.Codigo;
                        worksheet.Cells[row, 2].Value = detalle.Descripcion;
                        worksheet.Cells[row, 3].Value = detalle.Cantidad;
                        worksheet.Cells[row, 4].Value = detalle.ValorUnitario;
                        worksheet.Cells[row, 5].Value = detalle.PrecioUnitario;
                        worksheet.Cells[row, 6].Value = detalle.Subtotal;
                        worksheet.Cells[row, 7].Value = detalle.Igv;
                        worksheet.Cells[row, 8].Value = detalle.Total;
                        row++;
                    }
                }

                // Fila vacía para separar ventas
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteDiario_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }

        [HttpGet("reporte-noviembre")]
        public async Task<IActionResult> ReporteNoviembre()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            // Rango de fechas del mes de noviembre
            DateTime inicio = new DateTime(DateTime.Now.Year, 11, 1);
            DateTime fin = new DateTime(DateTime.Now.Year, 11, 30);

            // ⬅️ Aquí llamas a un método que devuelva las ventas de TODO el mes
            var ventas = await _facturacionService.GenerarReporteMensual(5, inicio, fin);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Reporte Noviembre");


            // Cabecera general
            ws.Cells[1, 1].Value = "Documento";
            ws.Cells[1, 2].Value = "Serie";
            ws.Cells[1, 3].Value = "Numero";
            ws.Cells[1, 4].Value = "Fecha Emisión";
            ws.Cells[1, 5].Value = "Sunat Anulado";
            ws.Cells[1, 6].Value = "Forma Pago";
            ws.Cells[1, 7].Value = "Cliente";
            ws.Cells[1, 8].Value = "Tipo";
            ws.Cells[1, 9].Value = "Código";
            ws.Cells[1, 10].Value = "Descripción";
            ws.Cells[1, 11].Value = "Cantidad";
            ws.Cells[1, 12].Value = "Valor Unit.";
            ws.Cells[1, 13].Value = "Subtotal";
            ws.Cells[1, 14].Value = "IGV";
            ws.Cells[1, 15].Value = "Total";
            ws.Cells[1, 16].Value = "Hora";

            using (var range = ws.Cells[1, 1, 1, 14])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;

            foreach (var venta in ventas)
            {
                foreach (var detalle in venta.Detalles)
                {
                    ws.Cells[row, 1].Value = $"{venta.TipoComprobante} {venta.Serie}-{venta.Numero}";
                    ws.Cells[row, 2].Value = venta.Serie;
                    ws.Cells[row, 3].Value = venta.Numero;
                    ws.Cells[row, 4].Value = venta.FechaEmision.ToString("dd/MM/yyyy");
                    ws.Cells[row, 5].Value = venta.EstadoSunat;
                    ws.Cells[row, 6].Value = venta.MetodoPago;
                    ws.Cells[row, 7].Value = venta.ClienteNombre;
                    ws.Cells[row, 8].Value = "NIÑO";
                    ws.Cells[row, 9].Value = detalle.Codigo;
                    ws.Cells[row, 10].Value = detalle.Descripcion;
                    ws.Cells[row, 11].Value = detalle.Cantidad;
                    ws.Cells[row, 12].Value = detalle.ValorUnitario;
                    ws.Cells[row, 13].Value = detalle.Subtotal;
                    ws.Cells[row, 14].Value = detalle.Igv;
                    ws.Cells[row, 15].Value = detalle.Total;
                    ws.Cells[row, 16].Value = venta.FechaEmision.ToString("HH:mm");
                    row++;
                }
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteDiario_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }


        [HttpGet("reporte-diario2")]
        public async Task<IActionResult> ReporteDiario2([FromQuery] DateTime fecha)
        {
            // Solo necesario si usas EPPlus <=7.2.2
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);


            var ventas = await _facturacionService.GenerarReporteDiario(establishmentId, fecha);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Reporte Diario");

            // Cabecera general
            ws.Cells[1, 1].Value = "Documento";
            ws.Cells[1, 2].Value = "Serie";
            ws.Cells[1, 3].Value = "Numero";
            ws.Cells[1, 4].Value = "Fecha Emisión";
            ws.Cells[1, 5].Value = "Sunat Anulado";
            ws.Cells[1, 6].Value = "Forma Pago";
            ws.Cells[1, 7].Value = "Cliente";
            ws.Cells[1, 8].Value = "Tipo";
            ws.Cells[1, 9].Value = "Código";
            ws.Cells[1, 10].Value = "Descripción";
            ws.Cells[1, 11].Value = "Cantidad";
            ws.Cells[1, 12].Value = "Valor Unit.";
            ws.Cells[1, 13].Value = "Subtotal";
            ws.Cells[1, 14].Value = "IGV";
            ws.Cells[1, 15].Value = "Total";
            ws.Cells[1, 16].Value = "Hora";

            using (var range = ws.Cells[1, 1, 1, 14])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;

            foreach (var venta in ventas)
            {
                foreach (var detalle in venta.Detalles)
                {
                    ws.Cells[row, 1].Value = $"{venta.TipoComprobante} {venta.Serie}-{venta.Numero}";
                    ws.Cells[row, 2].Value = venta.Serie;
                    ws.Cells[row, 3].Value = venta.Numero;
                    ws.Cells[row, 4].Value = venta.FechaEmision.ToString("dd/MM/yyyy");
                    ws.Cells[row, 5].Value = venta.EstadoSunat; 
                    ws.Cells[row, 6].Value = venta.MetodoPago;
                    ws.Cells[row, 7].Value = venta.ClienteNombre;
                    ws.Cells[row, 8].Value = "NIÑO"; 
                    ws.Cells[row, 9].Value = detalle.Codigo;
                    ws.Cells[row, 10].Value = detalle.Descripcion;
                    ws.Cells[row, 11].Value = detalle.Cantidad;
                    ws.Cells[row, 12].Value = detalle.ValorUnitario;
                    ws.Cells[row, 13].Value = detalle.Subtotal;
                    ws.Cells[row, 14].Value = detalle.Igv;
                    ws.Cells[row, 15].Value = detalle.Total;
                    ws.Cells[row, 16].Value = venta.FechaEmision.ToString("HH:mm");
                    row++;
                }
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteDiario_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }

    }
}
