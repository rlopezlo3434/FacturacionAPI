using FacturacionAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnulacionController : ControllerBase
    {
        private static readonly int[] EstablishmentIds = { 3, 4, 5, 6 };
        private readonly SistemaVentasDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AnulacionController> _logger;

        public AnulacionController(
            SistemaVentasDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<AnulacionController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Envía manualmente todas las anulaciones pendientes a Nubefact para todas las tiendas.
        /// </summary>
        [HttpPost("enviar-pendientes")]
        public async Task<IActionResult> EnviarAnulacionesPendientes(CancellationToken ct)
        {
            var resultados = new List<object>();

            foreach (var estId in EstablishmentIds)
            {
                var resultado = await ProcesarTiendaAsync(estId, ct);
                resultados.Add(resultado);
            }

            return Ok(resultados);
        }

        /// <summary>
        /// Envía manualmente las anulaciones pendientes de una tienda específica.
        /// </summary>
        [HttpPost("enviar-pendientes/{establishmentId:int}")]
        public async Task<IActionResult> EnviarAnulacionesPorTienda(int establishmentId, CancellationToken ct)
        {
            var resultado = await ProcesarTiendaAsync(establishmentId, ct);
            return Ok(resultado);
        }

        private async Task<object> ProcesarTiendaAsync(int establishmentId, CancellationToken ct)
        {
            var establishment = await _context.Establishment.FindAsync(new object[] { establishmentId }, ct);
            if (establishment == null)
            {
                return new { establishmentId, error = "Tienda no encontrada.", enviadas = 0, errores = 0 };
            }

            var pendientes = await _context.AnulacionDocumento
                .Include(a => a.venta)
                .Where(a => !a.EnviadoNubefact && a.venta.EstablishmentId == establishmentId)
                .ToListAsync(ct);

            if (!pendientes.Any())
            {
                return new { establishmentId, mensaje = "Sin anulaciones pendientes.", enviadas = 0, errores = 0 };
            }

            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", establishment.TokenNubefact);

            int enviadas = 0, errores = 0;
            var detalles = new List<object>();

            foreach (var anulacion in pendientes)
            {
                try
                {
                    var documento = anulacion.venta;

                    var payload = JsonSerializer.Serialize(new
                    {
                        operacion = "generar_anulacion",
                        tipo_de_comprobante = documento.TipoComprobante == "BOLETA" ? 2 : 1,
                        serie = documento.Serie,
                        numero = documento.Numero,
                        motivo = anulacion.Motivo,
                        codigo_unico = ""
                    });

                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    var response = await http.PostAsync(establishment.urlNubefact, content, ct);
                    var result = await response.Content.ReadAsStringAsync(ct);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Nubefact rechazó anulación VentaId={VentaId}: {Result}", anulacion.VentaId, result);
                        errores++;
                        detalles.Add(new { anulacion.VentaId, error = result });
                        continue;
                    }

                    var json = JsonSerializer.Deserialize<JsonElement>(result);
                    anulacion.EnlacePdf = json.GetProperty("enlace_del_pdf").GetString() ?? "";
                    anulacion.EnlaceXml = json.GetProperty("enlace_del_xml").GetString() ?? "";
                    anulacion.EnlaceCdr = json.GetProperty("enlace_del_cdr").GetString() ?? "";
                    anulacion.EnviadoNubefact = true;

                    enviadas++;
                    detalles.Add(new { anulacion.VentaId, estado = "OK" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar anulación VentaId={VentaId}.", anulacion.VentaId);
                    errores++;
                    detalles.Add(new { anulacion.VentaId, error = ex.Message });
                }
            }

            await _context.SaveChangesAsync(ct);

            return new { establishmentId, enviadas, errores, detalles };
        }
    }
}
