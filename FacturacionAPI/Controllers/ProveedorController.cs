using FacturacionAPI.Models.Entities;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService _proveedorService;

        public ProveedorController(ProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var proveedores = await _proveedorService.ObtenerTodos();
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var proveedor = await _proveedorService.ObtenerPorId(id);

            if (proveedor == null)
                return NotFound();

            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Proveedor proveedor)
        {
            var nuevo = await _proveedorService.Crear(proveedor);
            return Ok(nuevo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Proveedor proveedor)
        {
            var actualizado = await _proveedorService.Actualizar(id, proveedor);

            if (actualizado == null)
                return NotFound();

            return Ok(actualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _proveedorService.Eliminar(id);

            if (!eliminado)
                return NotFound();

            return Ok();
        }
    }
}
