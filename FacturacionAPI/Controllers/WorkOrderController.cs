using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController : ControllerBase
    {
        private readonly WorkOrderService _service;

        public WorkOrderController(WorkOrderService service)
        {
            _service = service;
        }

        // ✅ POST api/WorkOrder/create-from-intake/15
        [HttpPost("create-from-intake/{intakeId}")]
        public async Task<IActionResult> CreateFromIntake(int intakeId)
        {
            var result = await _service.CreateWorkOrderFromOfficialBudgetAsync(intakeId);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("from-intake/{intakeId}")]
        public async Task<IActionResult> GenerateWorkOrder(int intakeId)
        {
            var result = await _service.GenerateOrUpdateWorkOrderAsync(intakeId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ GET api/WorkOrder
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetWorkOrdersAsync();
            return Ok(new { success = true, data = result });
        }

        // ✅ GET api/WorkOrder/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetWorkOrderDetailAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "Orden de trabajo no encontrada." });

            return Ok(new { success = true, data = result });
        }

        // ✅ POST api/WorkOrder/{id}/liberar-facturacion
        [HttpPost("{id}/liberar-facturacion")]
        public async Task<IActionResult> LiberarFacturacion(int id)
        {
            var result = await _service.LiberarFacturacionAsync(id);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ GET api/WorkOrder/reporte-excel
        [HttpGet("reporte-excel")]
        public async Task<IActionResult> ReporteExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = await _service.GetWorkOrdersReporteAsync();

            using var package = new ExcelPackage();

            // ================= HOJA 1: RESUMEN (solo cabecera, una fila por OT) =================
            var wsResumen = package.Workbook.Worksheets.Add("Resumen");

            string[] headersResumen = { "Código OT", "Internamiento", "Fecha Creación", "Cliente", "Placa", "Marca", "Modelo", "Modo", "Estado", "N° Ítems", "Moneda", "Total" };
            for (int c = 0; c < headersResumen.Length; c++)
                wsResumen.Cells[1, c + 1].Value = headersResumen[c];

            using (var range = wsResumen.Cells[1, 1, 1, headersResumen.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int rowResumen = 2;
            foreach (var wo in data)
            {
                wsResumen.Cells[rowResumen, 1].Value = wo.Code;
                wsResumen.Cells[rowResumen, 2].Value = wo.Correlativo;
                wsResumen.Cells[rowResumen, 3].Value = wo.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                wsResumen.Cells[rowResumen, 4].Value = wo.ClienteNombre;
                wsResumen.Cells[rowResumen, 5].Value = wo.Placa;
                wsResumen.Cells[rowResumen, 6].Value = wo.Marca;
                wsResumen.Cells[rowResumen, 7].Value = wo.Modelo;
                wsResumen.Cells[rowResumen, 8].Value = wo.Mode == 1 ? "Taller" : "Recojo";
                wsResumen.Cells[rowResumen, 9].Value = wo.IsCompleted ? "Completada" : "Pendiente";
                wsResumen.Cells[rowResumen, 10].Value = wo.Items.Count;
                wsResumen.Cells[rowResumen, 11].Value = wo.Moneda == "USD" ? "$" : "S/";
                wsResumen.Cells[rowResumen, 12].Value = wo.Total;
                wsResumen.Cells[rowResumen, 12].Style.Numberformat.Format = "#,##0.00";
                rowResumen++;
            }

            if (wsResumen.Dimension != null)
                wsResumen.Cells[wsResumen.Dimension.Address].AutoFitColumns();

            // ================= HOJA 2: DETALLE (cabecera repetida por cada línea) =================
            var wsDetalle = package.Workbook.Worksheets.Add("Detalle");

            string[] headersDetalle = { "Código OT", "Internamiento", "Fecha Creación", "Cliente", "Placa", "Marca", "Modelo", "Modo", "Estado OT", "Moneda OT", "Total OT", "Tipo Ítem", "Descripción", "Cantidad", "Precio Unit.", "Descuento", "Subtotal", "Completado", "Observaciones" };
            for (int c = 0; c < headersDetalle.Length; c++)
                wsDetalle.Cells[1, c + 1].Value = headersDetalle[c];

            using (var range = wsDetalle.Cells[1, 1, 1, headersDetalle.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            int rowDetalle = 2;
            foreach (var wo in data)
            {
                var lineas = wo.Items.Any() ? wo.Items : new List<WorkOrderReporteItemDto> { null };

                foreach (var item in lineas)
                {
                    wsDetalle.Cells[rowDetalle, 1].Value = wo.Code;
                    wsDetalle.Cells[rowDetalle, 2].Value = wo.Correlativo;
                    wsDetalle.Cells[rowDetalle, 3].Value = wo.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                    wsDetalle.Cells[rowDetalle, 4].Value = wo.ClienteNombre;
                    wsDetalle.Cells[rowDetalle, 5].Value = wo.Placa;
                    wsDetalle.Cells[rowDetalle, 6].Value = wo.Marca;
                    wsDetalle.Cells[rowDetalle, 7].Value = wo.Modelo;
                    wsDetalle.Cells[rowDetalle, 8].Value = wo.Mode == 1 ? "Taller" : "Recojo";
                    wsDetalle.Cells[rowDetalle, 9].Value = wo.IsCompleted ? "Completada" : "Pendiente";
                    wsDetalle.Cells[rowDetalle, 10].Value = wo.Moneda == "USD" ? "$" : "S/";
                    wsDetalle.Cells[rowDetalle, 11].Value = wo.Total;
                    wsDetalle.Cells[rowDetalle, 11].Style.Numberformat.Format = "#,##0.00";

                    if (item != null)
                    {
                        wsDetalle.Cells[rowDetalle, 12].Value = item.Tipo;
                        wsDetalle.Cells[rowDetalle, 13].Value = item.Descripcion;
                        wsDetalle.Cells[rowDetalle, 14].Value = item.Cantidad;
                        wsDetalle.Cells[rowDetalle, 15].Value = item.PrecioUnitario;
                        wsDetalle.Cells[rowDetalle, 15].Style.Numberformat.Format = "#,##0.00";
                        wsDetalle.Cells[rowDetalle, 16].Value = item.Descuento;
                        wsDetalle.Cells[rowDetalle, 16].Style.Numberformat.Format = "#,##0.00";
                        wsDetalle.Cells[rowDetalle, 17].Value = item.Subtotal;
                        wsDetalle.Cells[rowDetalle, 17].Style.Numberformat.Format = "#,##0.00";
                        wsDetalle.Cells[rowDetalle, 18].Value = item.IsCompleted ? "Sí" : "No";
                        wsDetalle.Cells[rowDetalle, 19].Value = item.Observaciones ?? "";
                    }

                    rowDetalle++;
                }
            }

            if (wsDetalle.Dimension != null)
                wsDetalle.Cells[wsDetalle.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteOT_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }

        // ✅ PUT api/WorkOrder/update-items
        [HttpPut("update-items")]
        public async Task<IActionResult> UpdateItems([FromBody] WorkOrderUpdateItemsDto dto)
        {
            var result = await _service.UpdateWorkOrderItemsAsync(dto);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
