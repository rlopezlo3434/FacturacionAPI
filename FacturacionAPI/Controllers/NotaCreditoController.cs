using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/nota-credito")]
    //[Authorize]
    public class NotaCreditoController : ControllerBase
    {
        private readonly NotaCreditoService _service;

        public NotaCreditoController(NotaCreditoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Genera una Nota de Crédito por anulación de la operación.
        /// </summary>
        /// <param name="ventaId">ID de la venta original (boleta o factura) a anular.</param>
        /// <param name="reemplazadoPor">
        ///   Opcional. Serie y número del documento que reemplaza al anulado (ej: "F002-00000015").
        ///   Si no se envía, la NC se genera como una simple anulación de la operación.
        /// </param>
        [HttpPost("generar/{ventaId}")]
        public async Task<IActionResult> Generar(int ventaId, [FromQuery] string? reemplazadoPor = null)
        {
            try
            {
                var result = await _service.GenerarNotaCreditoAsync(ventaId, reemplazadoPor);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lista las notas de crédito de un establecimiento en un rango de fechas.
        /// </summary>
        [HttpGet("listar")]
        public async Task<IActionResult> Listar(
            [FromQuery] int establishmentId,
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                var result = await _service.ListarNotasCredito(establishmentId, desde, hasta);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
