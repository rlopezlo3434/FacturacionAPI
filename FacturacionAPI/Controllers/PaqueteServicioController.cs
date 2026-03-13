using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaqueteServicioController : ControllerBase
    {
        private readonly PaqueteServicioService _service;

        public PaqueteServicioController(PaqueteServicioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
                return NotFound(new { message = "Paquete no encontrado." });

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ServicePackageDto dto)
        {
            var result = await _service.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ServicePackageDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
