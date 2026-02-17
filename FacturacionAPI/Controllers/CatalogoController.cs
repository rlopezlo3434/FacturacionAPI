using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SistemaVentasDbContext _context;
        private readonly VehicleCatalogService _service;
        public CatalogoController(IConfiguration config, SistemaVentasDbContext context, VehicleCatalogService service)
        {
            _config = config;
            _context = context;
            _service = service;
        }

        [HttpGet("document-identification-types")]
        public IActionResult Get()
        {
            var documentIdentificationTypes = Enum.GetValues(typeof(Models.Enums.DocumentIdentificationType))
                .Cast<Models.Enums.DocumentIdentificationType>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                });

            return Ok(documentIdentificationTypes);
        }

        [HttpGet("gender-types")]
        public IActionResult GetGender()
        {
            var genderTypes = Enum.GetValues(typeof(Models.Enums.GenderEnum))
                .Cast<Models.Enums.GenderEnum>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                });

            return Ok(genderTypes);
        }

        [HttpGet("contact-types")]
        public IActionResult GetContactType()
        {
            var contactType = Enum.GetValues(typeof(Models.Enums.ContactTypeEnum))
                .Cast<Models.Enums.ContactTypeEnum>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                });
            return Ok(contactType);
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var data = await _service.GetBrandsAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("brands/{brandId}/models")]
        public async Task<IActionResult> GetModelsByBrand(int brandId)
        {
            var data = await _service.GetModelsByBrandAsync(brandId);
            return Ok(new { success = true, data });
        }

        [HttpPost("brands")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var result = await _service.CreateBrandAsync(dto.Name);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });
            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("brands/models")]
        public async Task<IActionResult> CreateModel([FromBody] CreateModelDto dto)
        {
            var result = await _service.CreateModelAsync(dto.BrandId, dto.Name, dto.IsActive);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });
            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet("unit-measures")]
        public async Task<IActionResult> GetUnitMeasures()
        {
            var data = await _service.GetUnitMeasuresAsync();
            return Ok(new { success = true, data });
        }
    }
}
