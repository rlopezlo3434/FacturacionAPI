using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardOwnerController : ControllerBase
    {
        private readonly DashboardOwnerService _service;

        public DashboardOwnerController(DashboardOwnerService service)
        {
            _service = service;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> Kpis(DateTime fecha)
            => Ok(await _service.GetKpis(fecha));

        [HttpGet("ventas-por-tienda")]
        public async Task<IActionResult> VentasPorTienda(DateTime fecha)
            => Ok(await _service.GetVentasPorTienda(fecha));

        [HttpGet("ventas-acumuladas")]
        public async Task<IActionResult> VentasAcumuladas(DateTime fecha)
            => Ok(await _service.GetVentasAcumuladas(fecha));

        [HttpGet("desviacion-por-tienda")]
        public async Task<IActionResult> Desviacion(DateTime fecha)
            => Ok(await _service.GetDesviacionPorTienda(fecha));
    }
}
