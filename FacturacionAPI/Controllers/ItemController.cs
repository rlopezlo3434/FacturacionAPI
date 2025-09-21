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

            var item = await _itemsService.CreateItemAsync(dto);

            return Ok(new
            {
                Success = true,
                Message = "Item creado exitosamente",
                Data = item
            });
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
    }
}
