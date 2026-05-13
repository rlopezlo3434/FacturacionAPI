using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompraController : ControllerBase
    {
        private readonly CompraService _service;

        public CompraController(CompraService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound(new { success = false, message = "Compra no encontrada" });
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompraCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest(new { success = false, message = "Debe incluir al menos un producto en el detalle" });

            var result = await _service.CreateAsync(dto);
            return Ok(new { success = true, data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Compra no encontrada" });
            return Ok(new { success = true });
        }
    }
}
