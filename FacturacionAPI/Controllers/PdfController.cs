using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfService _pdfService;

        public PdfController(PdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpGet("{id}/presupuesto")]
        public async Task<IActionResult> Test(int id)
        {
            var pdf = await _pdfService.GenerarPdfTest(id);

            return File(pdf, "application/pdf", "test.pdf");
        }

        [HttpGet("{id}/internamiento")]
        public async Task<IActionResult> PdfInternamiento(int id)
        {
            var pdf = await _pdfService.GenerarPdfInternamiento(id);

            return File(pdf, "application/pdf", "test.pdf");
        }

        [HttpGet("{id}/ordenTrabajo")]
        public async Task<IActionResult> PdfOrdenTrabajo(int id)
        {
            var pdf = await _pdfService.GenerarPdfOrdenTrabajo(id);

            return File(pdf, "application/pdf", "test.pdf");
        }

        [HttpGet("{id}/factura")]
        public async Task<IActionResult> PdfFactura(int id)
        {
            var pdf = await _pdfService.GenerarPdfFactura(id);

            return File(pdf, "application/pdf", "test.pdf");
        }
    }
}

