using FacturacionAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FacturacionAPI.Services
{
    public class AnulacionNubefactJob : BackgroundService
    {
        private static readonly int[] EstablishmentIds = { 3, 4, 5, 6 };
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AnulacionNubefactJob> _logger;

        public AnulacionNubefactJob(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<AnulacionNubefactJob> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TiempoHasta3Am();
                _logger.LogInformation("AnulacionNubefactJob: próxima ejecución en {minutos} minutos.", (int)delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await ProcesarAnulacionesPorTiendaAsync(stoppingToken);
            }
        }

        private static TimeSpan TiempoHasta3Am()
        {
            var ahora = DateTime.Now;
            var proximas3am = ahora.Date.AddHours(3);

            if (proximas3am <= ahora)
                proximas3am = proximas3am.AddDays(1);

            return proximas3am - ahora;
        }

        private async Task ProcesarAnulacionesPorTiendaAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SistemaVentasDbContext>();
            var http = _httpClientFactory.CreateClient();

            foreach (var estId in EstablishmentIds)
            {
                try
                {
                    await ProcesarTiendaAsync(context, http, estId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar anulaciones de tienda {EstablishmentId}.", estId);
                }
            }
        }

        private async Task ProcesarTiendaAsync(
            SistemaVentasDbContext context,
            HttpClient http,
            int establishmentId,
            CancellationToken ct)
        {
            var establishment = await context.Establishment.FindAsync(new object[] { establishmentId }, ct);
            if (establishment == null)
            {
                _logger.LogWarning("Tienda {EstablishmentId} no encontrada.", establishmentId);
                return;
            }

            var pendientes = await context.AnulacionDocumento
                .Include(a => a.venta)
                .Where(a => !a.EnviadoNubefact && a.venta.EstablishmentId == establishmentId)
                .ToListAsync(ct);

            if (!pendientes.Any())
            {
                _logger.LogInformation("Tienda {EstablishmentId}: sin anulaciones pendientes.", establishmentId);
                return;
            }

            _logger.LogInformation("Tienda {EstablishmentId}: enviando {Count} anulaciones a Nubefact.", establishmentId, pendientes.Count);

            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", establishment.TokenNubefact);

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
                        continue;
                    }

                    var json = JsonSerializer.Deserialize<JsonElement>(result);
                    anulacion.EnlacePdf = json.GetProperty("enlace_del_pdf").GetString() ?? "";
                    anulacion.EnlaceXml = json.GetProperty("enlace_del_xml").GetString() ?? "";
                    anulacion.EnlaceCdr = json.GetProperty("enlace_del_cdr").GetString() ?? "";
                    anulacion.EnviadoNubefact = true;

                    _logger.LogInformation("VentaId={VentaId} anulada en Nubefact correctamente.", anulacion.VentaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar anulación VentaId={VentaId}.", anulacion.VentaId);
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
