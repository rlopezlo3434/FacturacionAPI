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

        // Si la fecha es del mes actual → MTD (del 1 hasta ese día).
        // Si la fecha es de un mes pasado → mes completo.
        // El mes anterior sigue la misma lógica (MTD vs mes completo).
        private (DateTime inicioActual, DateTime finActual, DateTime inicioAnterior, DateTime finAnterior) ObtenerRangos(DateTime fecha)
        {
            var hoy = DateTime.Today;
            bool esMesActual = fecha.Year == hoy.Year && fecha.Month == hoy.Month;

            var inicioActual = new DateTime(fecha.Year, fecha.Month, 1);
            var inicioAnterior = inicioActual.AddMonths(-1);

            DateTime finActual, finAnterior;

            if (esMesActual)
                finActual = fecha.Date.AddDays(1).AddTicks(-1);
            else
                finActual = new DateTime(fecha.Year, fecha.Month,
                    DateTime.DaysInMonth(fecha.Year, fecha.Month), 23, 59, 59, 999);

            // Mes anterior siempre es el mes completo
            finAnterior = new DateTime(inicioAnterior.Year, inicioAnterior.Month,
                DateTime.DaysInMonth(inicioAnterior.Year, inicioAnterior.Month), 23, 59, 59, 999);

            return (inicioActual, finActual, inicioAnterior, finAnterior);
        }

        public async Task<OwnerKpisDto> GetKpis(DateTime fecha)
        {
            var (inicioMesActual, finMesActual, inicioMesAnterior, finMesAnterior) = ObtenerRangos(fecha);

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
            var (inicioActual, finActual, _, _) = ObtenerRangos(fecha);

            var ventas = await VentasValidas()
                .Include(v => v.Establishment)
                .Include(v => v.Detalles)
                .Where(v => v.FechaEmision >= inicioActual && v.FechaEmision <= finActual)
                .ToListAsync();

            return ventas
                .GroupBy(v => v.Establishment.Name)
                .Select(g => new VentasPorTiendaDto
                {
                    Tienda = g.Key,
                    Total = g.Sum(x => x.Total),
                    Servicios = (int)g.SelectMany(x => x.Detalles).Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.Total)
                .ToList();
        }

        public async Task<List<VentasAcumuladasDto>> GetVentasAcumuladas(DateTime fecha)
        {
            var (inicioActual, finActual, inicioAnterior, finAnterior) = ObtenerRangos(fecha);

            return await VentasValidas()
                .Where(v => (v.FechaEmision >= inicioActual && v.FechaEmision <= finActual) ||
                            (v.FechaEmision >= inicioAnterior && v.FechaEmision <= finAnterior))
                .GroupBy(v => v.FechaEmision.Day)
                .Select(g => new VentasAcumuladasDto
                {
                    Dia = g.Key,
                    MesActual = g.Where(x => x.FechaEmision >= inicioActual && x.FechaEmision <= finActual).Sum(x => x.Total),
                    MesAnterior = g.Where(x => x.FechaEmision >= inicioAnterior && x.FechaEmision <= finAnterior).Sum(x => x.Total)
                })
                .OrderBy(x => x.Dia)
                .ToListAsync();
        }

        public async Task<List<DesviacionTiendaDto>> GetDesviacionPorTienda(DateTime fecha)
        {
            var (inicioActual, finActual, inicioAnterior, finAnterior) = ObtenerRangos(fecha);

            return await _context.Establishment
                .Select(e => new DesviacionTiendaDto
                {
                    Tienda = e.Name,
                    Diferencia =
                        VentasValidas().Where(v => v.EstablishmentId == e.Id &&
                            v.FechaEmision >= inicioActual && v.FechaEmision <= finActual).Sum(v => (decimal?)v.Total) -
                        VentasValidas().Where(v => v.EstablishmentId == e.Id &&
                            v.FechaEmision >= inicioAnterior && v.FechaEmision <= finAnterior).Sum(v => (decimal?)v.Total)
                        ?? 0
                })
                .ToListAsync();
        }
    }
}
