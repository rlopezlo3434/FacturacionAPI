using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class DashboardOwnerService
    {
        private readonly SistemaVentasDbContext _context;

        public DashboardOwnerService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        private IQueryable<Venta> VentasValidas()
        {
            return _context.Ventas.Where(v => !v.IsAnnulled);
        }

        public async Task<OwnerKpisDto> GetKpis(DateTime fecha)
        {
            // ============================================
            // Rangos de fechas
            // Ejemplo: hoy = 4 de febrero
            // Mes actual    : 1–4 febrero
            // Mes anterior  : 1–4 enero
            // ============================================

            var inicioMesActual = new DateTime(fecha.Year, fecha.Month, 1);
            var finMesActual = fecha.Date.AddDays(1).AddTicks(-1); // fin del día

            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var finMesAnterior = inicioMesAnterior.AddDays(fecha.Day)
                                                  .AddTicks(-1);

            // ============================================
            // Ventas
            // ============================================

            var ventasActual = await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMesActual &&
                            v.FechaEmision <= finMesActual)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var ventasAnterior = await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMesAnterior &&
                            v.FechaEmision <= finMesAnterior)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            // ============================================
            // Servicios (misma lógica de fechas)
            // ============================================

            var serviciosActual = await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMesActual &&
                            v.FechaEmision <= finMesActual)
                .CountAsync();

            var serviciosAnterior = await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMesAnterior &&
                            v.FechaEmision <= finMesAnterior)
                .CountAsync();

            // ============================================
            // Cálculos derivados
            // ============================================

            var ticketPromedioActual =
                serviciosActual == 0 ? 0 : ventasActual / serviciosActual;

            var ticketPromedioAnterior =
                serviciosAnterior == 0 ? 0 : ventasAnterior / serviciosAnterior;

            var ticketVariacion =
                ticketPromedioAnterior == 0 ? 0 :
                ((ticketPromedioActual - ticketPromedioAnterior) / ticketPromedioAnterior) * 100;

            // ============================================
            // Resultado final
            // ============================================

            return new OwnerKpisDto
            {
                // Ventas
                VentasMtd = ventasActual,
                VentasVariacion = ventasAnterior == 0 ? 0 :
                    ((ventasActual - ventasAnterior) / ventasAnterior) * 100,
                DesviacionSoles = ventasActual - ventasAnterior,

                // Servicios
                ServiciosMtd = serviciosActual,
                ServiciosVariacion = serviciosAnterior == 0 ? 0 :
                    ((serviciosActual - serviciosAnterior) / (decimal)serviciosAnterior) * 100,

                // Ticket
                TicketPromedio = ticketPromedioActual,
                TicketVariacion = ticketVariacion
            };
        }

        public async Task<List<VentasPorTiendaDto>> GetVentasPorTienda(DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);

            return await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMes && v.FechaEmision <= fecha)
                .GroupBy(v => v.Establishment.Name)
                .Select(g => new VentasPorTiendaDto
                {
                    Tienda = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
        }

        public async Task<List<VentasAcumuladasDto>> GetVentasAcumuladas(DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);

            return await VentasValidas()
                .Where(v => v.FechaEmision >= inicioMesAnterior && v.FechaEmision <= fecha)
                .GroupBy(v => v.FechaEmision.Day)
                .Select(g => new VentasAcumuladasDto
                {
                    Dia = g.Key,
                    MesActual = g.Where(x => x.FechaEmision.Month == fecha.Month).Sum(x => x.Total),
                    MesAnterior = g.Where(x => x.FechaEmision.Month == inicioMesAnterior.Month).Sum(x => x.Total)
                })
                .OrderBy(x => x.Dia)
                .ToListAsync();
        }

        public async Task<List<DesviacionTiendaDto>> GetDesviacionPorTienda(DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var diaCorte = fecha.Day;

            return await _context.Establishment
                .Select(e => new DesviacionTiendaDto
                {
                    Tienda = e.Name,
                    Diferencia =
                        VentasValidas().Where(v => v.EstablishmentId == e.Id &&
                            v.FechaEmision >= inicioMes && v.FechaEmision <= fecha).Sum(v => (decimal?)v.Total) -
                        VentasValidas().Where(v => v.EstablishmentId == e.Id &&
                            v.FechaEmision >= inicioMesAnterior &&
                            v.FechaEmision < inicioMesAnterior.AddDays(diaCorte)).Sum(v => (decimal?)v.Total)
                        ?? 0
                })
                .ToListAsync();
        }
    }
}
