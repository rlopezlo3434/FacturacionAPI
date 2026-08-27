using FacturacionAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class EnvioDocumentosPendientesService
    {
        private readonly SistemaVentasDbContext _context;
        private readonly NotaCreditoService _ncService;

        public EnvioDocumentosPendientesService(
            SistemaVentasDbContext context,
            NotaCreditoService ncService)
        {
            _context = context;
            _ncService = ncService;
        }

        /// <summary>
        /// Envía todos los documentos pendientes cuya FechaProgramada ya llegó.
        /// Retorna un resumen de lo procesado.
        /// </summary>
        public async Task<EnvioResumenDto> EnviarPendientesAsync(int? establishmentId = null)
        {
            var query = _context.DocumentosPendientesEnvio
                .Where(d =>
                    d.Estado == "PENDIENTE");

            if (establishmentId.HasValue)
                query = query.Where(d => d.EstablishmentId == establishmentId.Value);

            var pendientes = await query
                .OrderBy(d => d.FechaProgramada)
                .ToListAsync();

            var resumen = new EnvioResumenDto();

            foreach (var doc in pendientes)
            {
                doc.Intentos++;

                try
                {
                    if (doc.TipoDocumento == "NOTA_CREDITO")
                    {
                        var (ok, error, ncVentaId) = await _ncService.EnviarNotaCreditoANubefactAsync(doc);

                        if (ok)
                        {
                            doc.Estado = "ENVIADO";
                            doc.EnviadoAt = DateTime.Now;
                            doc.NcVentaId = ncVentaId;
                            resumen.Enviados++;
                        }
                        else
                        {
                            doc.Estado = doc.Intentos >= 3 ? "ERROR" : "PENDIENTE";
                            doc.MensajeError = error;
                            resumen.Errores++;
                        }
                    }
                    else if (doc.TipoDocumento == "ANULACION")
                    {
                        var (ok, error) = await _ncService.EnviarAnulacionANubefactAsync(doc);

                        if (ok)
                        {
                            doc.Estado = "ENVIADO";
                            doc.EnviadoAt = DateTime.Now;
                            resumen.Enviados++;
                        }
                        else
                        {
                            doc.Estado = doc.Intentos >= 3 ? "ERROR" : "PENDIENTE";
                            doc.MensajeError = error;
                            resumen.Errores++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    doc.Estado = doc.Intentos >= 3 ? "ERROR" : "PENDIENTE";
                    doc.MensajeError = ex.Message;
                    resumen.Errores++;
                }

                await _context.SaveChangesAsync();
            }

            resumen.TotalProcesados = pendientes.Count;
            return resumen;
        }

        /// <summary>
        /// Reintenta manualmente un documento en estado ERROR.
        /// </summary>
        public async Task<(bool Success, string Message)> ReintentarAsync(int documentoId)
        {
            var doc = await _context.DocumentosPendientesEnvio
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (doc == null)
                return (false, "Documento no encontrado.");

            if (doc.Estado == "ENVIADO")
                return (false, "El documento ya fue enviado.");

            // Resetear para que lo tome en el próximo envío
            doc.Estado = "PENDIENTE";
            doc.FechaProgramada = DateTime.Now;
            doc.MensajeError = null;
            doc.Intentos = 0;

            await _context.SaveChangesAsync();

            return (true, "Documento marcado para reintento.");
        }
    }

    public class EnvioResumenDto
    {
        public int TotalProcesados { get; set; }
        public int Enviados { get; set; }
        public int Errores { get; set; }
    }
}
