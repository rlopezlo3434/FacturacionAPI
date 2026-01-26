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

        public VehicleIntakesController(VehicleIntakeService service)
        {
            _service = service;
        }

        // ✅ GET inventario maestro (plantilla)
        [HttpGet("inventory-master")]
        public async Task<IActionResult> GetInventoryMaster()
        {
            var data = await _service.GetInventoryMasterAsync();
            return Ok(new { success = true, data });
        }

        // ✅ POST crear internamiento
        [HttpPost]
        public async Task<IActionResult> CreateIntake([FromBody] CreateVehicleIntakeDto dto)
        {
            var result = await _service.CreateVehicleIntakeAsync(dto);

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
    }
}
