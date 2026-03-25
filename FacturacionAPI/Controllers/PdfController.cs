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
    }
}
