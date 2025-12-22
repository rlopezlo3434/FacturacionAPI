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
    public class ItemController : ControllerBase
    {
        private readonly ItemsService _itemsService;

        public ItemController(ItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Data = ModelState
                });
            }

            var result = await _itemsService.CreateItemAsync(dto);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new
            {
                Success = true,
                Message = "Item creado exitosamente"
            });
        }

        [HttpPost("updateItem")]
        public async Task<IActionResult> UpdateItem([FromBody] CreateItemDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos o ID de item no especificado.");

            var result = await _itemsService.UpdateItemAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpGet("items-by-establishment")]
        public async Task<IActionResult> getItemsByEstablishment()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var items = await _itemsService.getItemsByEstablishment(establishmentId);
            if (items == null || !items.Any())
                return NotFound(new
                {
                    Success = false,
                    Message = "No se encontraron items para este establecimiento",
                    Data = ModelState
                });

            return Ok(items);
        }

        [Authorize]
        [HttpPost("updateState/{id}")]
        public async Task<IActionResult> UpdateState(int id, [FromBody] UpdateStateRequest request)
        {
            if (request.IsActive == null)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var result = await _itemsService.UpdateItemStateAsync(id, request.IsActive);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new { Success = true, Message = "Estado actualizado correctamente" });
        }
    }
}
