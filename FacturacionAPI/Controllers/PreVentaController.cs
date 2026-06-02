using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreVentaController : ControllerBase
    {
        private readonly PreVentaService _preVentaService;

        public PreVentaController(PreVentaService preVentaService)
        {
            _preVentaService = preVentaService;
        }

        [Authorize]
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarPreVenta([FromBody] VentaRequest request)
        {
            if (request == null || request.items == null || !request.items.Any())
                return BadRequest(new { success = false, message = "La pre-venta no contiene ítems válidos." });

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _preVentaService.RegistrarPreVentaAsync(request, establishmentId);
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
        [HttpPost("emitir/{id}")]
        public async Task<IActionResult> EmitirPreVenta(int id, [FromQuery] string serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                return BadRequest(new { success = false, message = "Debe indicar la serie para emitir el comprobante." });

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _preVentaService.EmitirPreVentaAsync(id, serie, establishmentId);
                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, new { success = false, message = "Nubefact no respondió a tiempo. Verifique si el comprobante fue emitido antes de reintentar." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPreVenta(int id)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _preVentaService.EliminarPreVentaAsync(id, establishmentId);
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
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var resultado = await _preVentaService.GetPreVentasPendientes(establishmentId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}
