using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/nota-credito")]
    public class NotaCreditoController : ControllerBase
    {
        private readonly NotaCreditoService _service;
        private readonly EnvioDocumentosPendientesService _envioService;
        private readonly PdfService _pdfService;

        public NotaCreditoController(
            NotaCreditoService service,
            EnvioDocumentosPendientesService envioService,
            PdfService pdfService)
        {
            _service = service;
            _envioService = envioService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Encola una NC para la venta indicada.
        /// Boleta → se envía al día siguiente. Factura → inmediato en el próximo dispatch.
        /// reemplazadoPor: serie-numero del comprobante nuevo (ej: "F002-15")
        /// </summary>
        [HttpPost("encolar/{ventaId}")]
        public async Task<IActionResult> Encolar(int ventaId, [FromQuery] string? reemplazadoPor = null)
        {
            var result = await _service.EncolarNotaCreditoAsync(ventaId, reemplazadoPor);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message, pendienteId = result.PendienteId });
        }

        /// <summary>
        /// Encola una Anulación para la venta indicada.
        /// Boleta → día siguiente. Factura → inmediato en próximo dispatch.
        /// </summary>
        [HttpPost("encolar-anulacion/{ventaId}")]
        public async Task<IActionResult> EncolarAnulacion(int ventaId)
        {
            var result = await _service.EncolarAnulacionAsync(ventaId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message, pendienteId = result.PendienteId });
        }

        /// <summary>
        /// Despacha todos los documentos pendientes cuya fecha programada ya llegó.
        /// Llamar desde el front en el login del día, o vía tarea programada.
        /// </summary>
        [HttpPost("enviar-pendientes")]
        public async Task<IActionResult> EnviarPendientes([FromQuery] int? establishmentId)
        {
            var resumen = await _envioService.EnviarPendientesAsync(establishmentId);
            return Ok(new { success = true, data = resumen });
        }

        /// <summary>
        /// Reintenta manualmente un documento en estado ERROR.
        /// </summary>
        [HttpPost("reintentar/{documentoId}")]
        public async Task<IActionResult> Reintentar(int documentoId)
        {
            var result = await _envioService.ReintentarAsync(documentoId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// Lista todos los documentos pendientes/enviados/error de un establecimiento.
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<IActionResult> ListarPendientes([FromQuery] int establishmentId)
        {
            var data = await _service.ListarPendientesAsync(establishmentId);
            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Lista las Notas de Crédito ya emitidas (enviadas a Nubefact) en un rango de fechas.
        /// </summary>
        [HttpGet("listar")]
        public async Task<IActionResult> Listar(
            [FromQuery] int establishmentId,
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            var data = await _service.ListarNotasCredito(establishmentId, desde, hasta);
            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Descarga el PDF local de una NC ya emitida (por ventaId de la NC).
        /// </summary>
        [HttpGet("{ventaId}/pdf")]
        public async Task<IActionResult> DescargarPdf(int ventaId)
        {
            var bytes = await _pdfService.GenerarPdfNotaCredito(ventaId);
            return File(bytes, "application/pdf", $"NC_{ventaId}.pdf");
        }
    }
}
