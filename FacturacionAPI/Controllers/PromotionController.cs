using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PromotionController : ControllerBase
    {
        private readonly PromotionService _promotionService;

        public PromotionController(PromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var result = await _promotionService.CreatePromotionAsync(dto, establishmentId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);
            var promotions = await _promotionService.GetPromotionsByEstablishmentAsync(establishmentId);
            return Ok(promotions);
        }

        [Authorize]
        [HttpPost("{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] PromotionUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            dto.Id = id;

            var result = await _promotionService.UpdatePromotionAsync(dto);

            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
