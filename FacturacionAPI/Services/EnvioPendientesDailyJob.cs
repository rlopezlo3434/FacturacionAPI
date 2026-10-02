using Microsoft.Extensions.Hosting;

namespace FacturacionAPI.Services
{
    /// <summary>
    /// Tarea programada que se activa UNA sola vez al día, a las 3:00 am,
    /// para enviar a Nubefact los documentos pendientes (Notas de Crédito y Anulaciones)
    /// que quedaron encolados en DocumentosPendientesEnvio.
    /// No hace polling continuo: duerme hasta la próxima 3am, ejecuta una vez, y vuelve a dormir.
    /// </summary>
    public class EnvioPendientesDailyJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EnvioPendientesDailyJob> _logger;

        public EnvioPendientesDailyJob(IServiceScopeFactory scopeFactory, ILogger<EnvioPendientesDailyJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TiempoHastaLas3Am();
                _logger.LogInformation("EnvioPendientesDailyJob: próxima ejecución en {minutos} minutos (03:00 am).", (int)delay.TotalMinutes);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (stoppingToken.IsCancellationRequested) break;

                await ProcesarPendientesAsync();
            }
        }

        private static TimeSpan TiempoHastaLas3Am()
        {
            var ahora = DateTime.Now;
            var hoyA3am = ahora.Date.AddHours(3);
            var proxima3am = ahora < hoyA3am ? hoyA3am : hoyA3am.AddDays(1);
            return proxima3am - ahora;
        }

        private async Task ProcesarPendientesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var envioService = scope.ServiceProvider.GetRequiredService<EnvioDocumentosPendientesService>();

            try
            {
                // null = procesa los pendientes de todos los establecimientos
                var resumen = await envioService.EnviarPendientesAsync(null);

                _logger.LogInformation(
                    "EnvioPendientesDailyJob: procesados {Total}, enviados {Enviados}, errores {Errores}.",
                    resumen.TotalProcesados, resumen.Enviados, resumen.Errores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnvioPendientesDailyJob: error al procesar documentos pendientes.");
            }
        }
    }
}
