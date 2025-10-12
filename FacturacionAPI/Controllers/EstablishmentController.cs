using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstablishmentController : ControllerBase
    {
        private readonly EstablishmentService _establishmentService;
        public EstablishmentController(EstablishmentService establishmentService)
        {
            _establishmentService = establishmentService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetEstablishment()
        {
            var establishments = await _establishmentService.GetEstablishment();
            if (establishments == null || !establishments.Any())
                return NotFound(new { Message = "No se encontraron empleados para este establecimiento" });

            return Ok(establishments);
        }

        //[Authorize]
        //[HttpPost]
        //public async Task
    }
}
