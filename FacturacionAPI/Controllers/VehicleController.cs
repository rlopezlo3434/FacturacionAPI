using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService _vehicleService;

        public VehicleController(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var result = await _vehicleService.GetVehiclesAsync();
            return Ok(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleCreateDto dto)
        {

            var result = await _vehicleService.CreateVehicleAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleUpdateDto dto)
        {

            var result = await _vehicleService.UpdateVehicleAsync(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpGet("reporte-vehiculos")]
        public async Task<IActionResult> ReporteVehiculos()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var vehiculos = await _vehicleService.GetVehiclesAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vehículos");

            int row = 1;

            // CABECERA
            worksheet.Cells[row, 1].Value = "ID";
            worksheet.Cells[row, 2].Value = "Placa";
            worksheet.Cells[row, 3].Value = "Marca";
            worksheet.Cells[row, 4].Value = "Modelo";
            worksheet.Cells[row, 5].Value = "Año";
            worksheet.Cells[row, 6].Value = "Color";
            worksheet.Cells[row, 7].Value = "Motor";
            worksheet.Cells[row, 8].Value = "VIN";
            worksheet.Cells[row, 9].Value = "Serie";
            worksheet.Cells[row, 10].Value = "Kilometraje";
            worksheet.Cells[row, 11].Value = "Propietario";
            worksheet.Cells[row, 12].Value = "Activo";

            using (var range = worksheet.Cells[row, 1, row, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            row++;

            foreach (var v in vehiculos)
            {
                worksheet.Cells[row, 1].Value = v.Id;
                worksheet.Cells[row, 2].Value = v.Plate;
                worksheet.Cells[row, 3].Value = v.Brand?.Name;
                worksheet.Cells[row, 4].Value = v.Model?.Name;
                worksheet.Cells[row, 5].Value = v.Year;
                worksheet.Cells[row, 6].Value = v.Color;
                worksheet.Cells[row, 7].Value = v.Motor;
                worksheet.Cells[row, 8].Value = v.Vin;
                worksheet.Cells[row, 9].Value = v.SerialNumber;
                worksheet.Cells[row, 10].Value = v.CurrentMileageKm;
                worksheet.Cells[row, 11].Value = v.Owner?.Name ?? "Sin propietario";
                worksheet.Cells[row, 12].Value = v.IsActive ? "Sí" : "No";

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteVehiculos_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }
    }
}
