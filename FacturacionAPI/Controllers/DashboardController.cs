using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("ventas-mensuales")]
        public async Task<IActionResult> GetVentasMensuales(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var data = await _dashboardService.GetVentasMensuales(establishmentId, fecha);

            return Ok(data);
        }

        [HttpGet("servicios-mensuales")]
        public async Task<IActionResult> GetServiciosMensuales(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var data = await _dashboardService.GetServiciosMensuales(establishmentId, fecha);

            return Ok(data);
        }

        [HttpGet("top-servicios-dia")]
        public async Task<IActionResult> TopServiciosDia(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetTopServiciosDia(5, fecha));
        }

        [HttpGet("top-servicios-mes")]
        public async Task<IActionResult> TopServiciosMes(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetTopServiciosMes(5, fecha));
        }


        [HttpGet("top-servicios-dia-cantidad")]
        public async Task<IActionResult> TopServiciosDiaCantidad(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetTopServiciosDiaCantidad(5, fecha));
        }

        [HttpGet("top-servicios-mes-cantidad")]
        public async Task<IActionResult> TopServiciosMesCantidad(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetTopServiciosMesCantidad(5, fecha));
        }

        [HttpGet("comparativo-mensual")]
        public async Task<IActionResult> ComparativoMensual(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetComparativoVentas(5, fecha));
        }

        [HttpGet("comparativo-diario-circle")]
        public async Task<IActionResult> ComparativoDiario(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetComparativoDiario(5,fecha));
        }

        [HttpGet("comparativo-mensual-circle")]
        public async Task<IActionResult> ComparativoMensual2(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetComparativoMensual(5,fecha));
        }

        [HttpGet("productividad-personal")]
        public async Task<IActionResult> ProductividadPersonal(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetProductividadPersonal(establishmentId, fecha));
        }

        [HttpGet("contribucion-estilista-dia")]
        public async Task<IActionResult> ContribucionDia(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetContribucionEstilistaDia(establishmentId, fecha));
        }

        [HttpGet("contribucion-estilista-mes")]
        public async Task<IActionResult> ContribucionMes(DateTime fecha)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            return Ok(await _dashboardService.GetContribucionEstilistaMes(establishmentId, fecha));
        }

    }
}
