using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleIntakesController : ControllerBase
    {
        private readonly VehicleIntakeService _service;
        private readonly IConfiguration _configuration;

        public VehicleIntakesController(VehicleIntakeService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        // ✅ GET inventario maestro (plantilla)
        [HttpGet("inventory-master")]
        public async Task<IActionResult> GetInventoryMaster()
        {
            var data = await _service.GetInventoryMasterAsync();
            return Ok(new { success = true, data });
        }

        // ✅ POST crear internamiento
        //[HttpPost]
        //public async Task<IActionResult> CreateIntake([FromBody] CreateVehicleIntakeDto dto)
        //{
        //    var result = await _service.CreateVehicleIntakeAsync(dto);

        //    if (!result.Success)
        //        return BadRequest(new { success = false, message = result.Message });

        //    return Ok(new { success = true, message = result.Message });
        //}

        [HttpPost]
        public async Task<IActionResult> CreateIntake([FromForm] CreateVehicleIntakeDto dto, [FromForm] List<IFormFile> images, List<IFormFile>? diagrams)
        {
            var result = await _service.CreateVehicleIntakeAsync(dto, images, diagrams);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetIntakes()
        {
            var data = await _service.GetIntakesAsync();
           
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var intake = await _service.GetIntakeDetailAsync(id);

            if (intake == null)
                return NotFound(new { success = false, message = "Internamiento no encontrado." });

            return Ok(new { success = true, data = intake });
        }

        [HttpGet("image/{intakeId:int}/{fileName}")]
        public IActionResult GetIntakeImage(int intakeId, string fileName)
        {
            // 📂 Ruta base desde configuración
            var basePath = _configuration["Storage:IntakesPath"]
                           ?? @"C:\xtremeMotors\Intakes";

            // 🔐 Seguridad básica contra path traversal
            fileName = Path.GetFileName(fileName);

            var fullPath = Path.Combine(
                basePath,
                intakeId.ToString(),
                fileName
            );

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            // 🧠 Detectar content-type
            var contentType = GetContentType(fullPath);

            // ⚡ Cache en navegador (1 día)
            Response.Headers["Cache-Control"] = "public,max-age=86400";

            return PhysicalFile(
                fullPath,
                contentType,
                enableRangeProcessing: true
            );
        }

        // 🔧 Helper para tipo MIME
        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
