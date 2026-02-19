using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using static FacturacionAPI.Models.DTOs.CajaDTO;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly CajaService _service;

        public CajaController(CajaService cajaService)
        {
            _service = cajaService;
        }

        // ------------------------
        // ABRIR CAJA
        // ------------------------
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja(CrearCaja request)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var r = await _service.AbrirCaja(establishmentId, request.monto);
            return Ok(r);
        }

        // ------------------------
        // REGISTRAR MOVIMIENTO MANUAL
        // ------------------------
        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] CajaMovimientoRequest mov)
        {
            var r = await _service.RegistrarMovimiento(mov.CajaAperturaId, mov.Monto, mov.Tipo, mov.Motivo);
            return Ok(r);
        }

        // ------------------------
        // REGISTRAR MOVIMIENTO POR VENTA
        // ------------------------
        [HttpPost("venta/{ventaId}")]
        public async Task<IActionResult> MovimientoPorVenta(int ventaId)
        {
            var r = await _service.RegistrarMovimientoPorVenta(ventaId);
            return Ok(r);
        }

        // ------------------------
        // OBTENER CAJA ABIERTA
        // ------------------------
        [HttpGet("abierta/{establishmentId}")]
        public async Task<IActionResult> CajaAbierta()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var r = await _service.ObtenerCajaAbierta(establishmentId);
            return Ok(r);
        }

        // ------------------------
        // CERRAR CAJA
        // ------------------------
        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequest body)
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var r = await _service.CerrarCaja(establishmentId, body.CajaAperturaId, body.EfectivoContado, body.Observaciones);
            return Ok(r);
        }

        // ------------------------
        // DETALLE COMPLETO
        // ------------------------
        [HttpGet("detalle/{cajaId}")]
        public async Task<IActionResult> Detalle(int cajaId)
        {
            var r = await _service.ObtenerDetalle(cajaId);
            return Ok(r);
        }

        [Authorize]
        [HttpGet("abiertas")]
        public async Task<IActionResult> ListarCajasAbiertas()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);
            var cajas = await _service.ListarCajasAbiertas(establishmentId);
            return Ok(cajas);
        }

        [HttpGet("excel/{cajaId}")]
        public async Task<IActionResult> DescargarExcel(int cajaId, [FromQuery] DateTime? fecha)
        {
            var excelData = await _service.GenerarExcelCaja(cajaId, fecha);

            return File(
                excelData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"reporte_caja_{cajaId}.xlsx"
            );
        }

        [HttpGet("excel-mensual")]
        public async Task<IActionResult> ReporteMensual([FromQuery] DateTime? fecha)
        {
            // Si no envían fecha, usar la actual
            DateTime fechaBase = fecha ?? DateTime.Now;

            int year = fechaBase.Year;
            int month = fechaBase.Month;

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var excelBytes = await _service.GenerarReporteMensual(establishmentId, year, month);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"reporte_caja_{year}_{month}.xlsx"
            );
        }


    }
}
