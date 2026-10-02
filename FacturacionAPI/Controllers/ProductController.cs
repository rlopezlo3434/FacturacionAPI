using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var data = await _service.GetProductsAsync();
            return Ok(new { success = true, data });
        }

        // ✅ GET: api/Product/kardex-salidas?productId=5
        [HttpGet("kardex-salidas")]
        public async Task<IActionResult> GetKardexSalidas([FromQuery] int? productId)
        {
            var data = await _service.GetKardexSalidasAsync(productId);
            return Ok(new { success = true, data });
        }

        // ✅ POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        {
            var result = await _service.CreateProductAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto dto)
        {
            var result = await _service.UpdateProductAsync(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }


}
