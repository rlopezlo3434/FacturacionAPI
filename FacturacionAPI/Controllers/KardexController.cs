using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KardexController : ControllerBase
    {
        private readonly KardexService _kardexService;

        public KardexController(KardexService kardexService)
        {
            _kardexService = kardexService;
        }

        [Authorize]
        [HttpGet("by-establishment")]
        public async Task<IActionResult> GetProductsByEstablishment()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var employees = await _kardexService.GetProductsByEstablishment(establishmentId);
            if (employees == null || !employees.Any())
                return NotFound(new { Message = "No se encontraron movimientos para este establecimiento" });

            return Ok(employees);
        }

        [HttpGet("movimientos/{itemId}")]
        public async Task<IActionResult> GetStockMovementsByItem(int itemId)
        {
            try
            {
                var result = await _kardexService.GetStockMovementsByItem(itemId);

                if (result == null || !result.Any())
                    return Ok(new { Success = false, Message = "No se encontraron movimientos para este producto." });

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Error al obtener los movimientos de stock.", Error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("movimientos")]
        public async Task<IActionResult> PostMovement([FromBody] StockMovementDto movement)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos enviados no son válidos.",
                    errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                });
            }

            var result = await _kardexService.PostMovement(movement);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = movement
            });
        }

    }
}
