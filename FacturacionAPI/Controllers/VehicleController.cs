using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

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

    }
}
