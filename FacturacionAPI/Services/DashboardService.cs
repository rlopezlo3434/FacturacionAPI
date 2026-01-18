using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class DashboardService
    {
        private readonly SistemaVentasDbContext _context;

        public DashboardService(SistemaVentasDbContext context)
        {
            _context = context;
        }
        public async Task<VentasMensualesDto> GetVentasMensuales(
            int establishmentId,
            DateTime fecha)
        {
            // Primer día del mes
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);

            // Primer día del mes siguiente
            var finMes = inicioMes.AddMonths(1);

            // Ventas del mes (NO anuladas)
            var ventasMes = await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < finMes &&
                    !v.IsAnnulled
                )
                .ToListAsync();

            // Agrupar por día
            var montosPorDia = ventasMes
                .GroupBy(v => v.FechaEmision.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Dia)
                .ToList();

            // Crear lista día 1 → último día (con ceros)
            int diasEnMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);

            var dias = Enumerable.Range(1, diasEnMes)
                .Select(d => d.ToString())
                .ToList();

            var montos = Enumerable.Range(1, diasEnMes)
                .Select(d =>
                    montosPorDia
                        .FirstOrDefault(x => x.Dia.Day == d)?.Total ?? 0
                )
                .ToList();

            // Total del día actual
            var totalDia = ventasMes
                .Where(v => v.FechaEmision.Date == fecha.Date)
                .Sum(v => v.Total);

            // Total acumulado del mes
            var totalMes = ventasMes.Sum(v => v.Total);

            return new VentasMensualesDto
            {
                Dias = dias,
                MontosPorDia = montos,
                TotalDia = totalDia,
                TotalMes = totalMes
            };
        }

        public async Task<ServiciosMensualesDto> GetServiciosMensuales(
            int establishmentId,
            DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var ventasMes = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < finMes &&
                    !v.IsAnnulled
                )
                .ToListAsync();

            // agrupar servicios por día
            var serviciosPorDia = ventasMes
                .GroupBy(v => v.FechaEmision.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Cantidad = g.Sum(x => x.Detalles.Sum(d => d.Cantidad))
                })
                .OrderBy(x => x.Dia)
                .ToList();

            int diasEnMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);

            var dias = Enumerable.Range(1, diasEnMes)
                .Select(d => d.ToString())
                .ToList();

            var listaServicios = Enumerable.Range(1, diasEnMes)
                .Select(d =>
                    serviciosPorDia.FirstOrDefault(x => x.Dia.Day == d)?.Cantidad ?? 0
                )
                .ToList();

            // total del día
            var totalDia = ventasMes
                .Where(v => v.FechaEmision.Date == fecha.Date)
                .Sum(v => v.Detalles.Sum(d => d.Cantidad));

            // total acumulado del mes
            var totalMes = ventasMes.Sum(v => v.Detalles.Sum(d => d.Cantidad));

            return new ServiciosMensualesDto
            {
                Dias = dias,
                ServiciosPorDia = listaServicios,
                TotalDia = totalDia,
                TotalMes = totalMes
            };
        }

        public async Task<List<TopServicioDto>> GetTopServiciosDia(
            int establishmentId,
            DateTime fecha)
        {
            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision.Date == fecha.Date &&
                    !v.IsAnnulled
                )
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Descripcion)
                .Select(g => new TopServicioDto
                {
                    Servicio = g.Key,
                    Monto = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Monto)
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<TopServicioDto>> GetTopServiciosMes(
            int establishmentId,
            DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < finMes &&
                    !v.IsAnnulled
                )
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Descripcion)
                .Select(g => new TopServicioDto
                {
                    Servicio = g.Key,
                    Monto = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Monto)
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<TopServicioDto>> GetTopServiciosDiaCantidad(
            int establishmentId,
            DateTime fecha)
        {
            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision.Date == fecha.Date &&
                    !v.IsAnnulled
                )
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Descripcion)
                .Select(g => new TopServicioDto
                {
                    Servicio = g.Key,
                    Monto = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Monto)
                .Take(5)
                .ToListAsync();
        }


        public async Task<List<TopServicioDto>> GetTopServiciosMesCantidad(
            int establishmentId,
            DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            return await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMes &&
                    v.FechaEmision < finMes &&
                    !v.IsAnnulled
                )
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Descripcion)
                .Select(g => new TopServicioDto
                {
                    Servicio = g.Key,
                    Monto = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Monto)
                .Take(5)
                .ToListAsync();
        }

        public async Task<ComparativoVentasDto> GetComparativoVentas(
            int establishmentId,
            DateTime fecha)
        {
            var inicioMesActual = new DateTime(fecha.Year, fecha.Month, 1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var inicioMesSiguiente = inicioMesActual.AddMonths(1);

            var ventasActual = await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMesActual &&
                    v.FechaEmision < inicioMesSiguiente &&
                    !v.IsAnnulled)
                .ToListAsync();

            var ventasAnterior = await _context.Ventas
                .Where(v =>
                    v.EstablishmentId == establishmentId &&
                    v.FechaEmision >= inicioMesAnterior &&
                    v.FechaEmision < inicioMesActual &&
                    !v.IsAnnulled)
                .ToListAsync();

            int diasMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);
            int diasMesAnterior = DateTime.DaysInMonth(inicioMesAnterior.Year, inicioMesAnterior.Month);

            var dias = Enumerable.Range(1, diasMes).Select(d => d.ToString()).ToList();

            var actual = Enumerable.Range(1, diasMes)
                .Select(d => ventasActual
                    .Where(v => v.FechaEmision.Day == d)
                    .Sum(v => v.Total))
                .ToList();

            var anterior = Enumerable.Range(1, diasMes)
                .Select(d => d <= diasMesAnterior
                    ? ventasAnterior
                        .Where(v => v.FechaEmision.Day == d)
                        .Sum(v => v.Total)
                    : 0)
                .ToList();

            return new ComparativoVentasDto
            {
                Dias = dias,
                MesActual = actual,
                MesAnterior = anterior
            };
        }

        public async Task<ComparativoResponse> GetComparativoDiario(int establishmentId, DateTime fecha)
        {
            var hoy = fecha.Date;
            var ayer = hoy.AddDays(-1);

            var totalHoy = await _context.Ventas
                .Where(x => x.FechaEmision.Date == hoy && x.EstablishmentId == establishmentId)
                .SumAsync(x => (decimal?)x.Total) ?? 0;

            var totalAyer = await _context.Ventas
                .Where(x => x.FechaEmision.Date == ayer && x.EstablishmentId == establishmentId)
                .SumAsync(x => (decimal?)x.Total) ?? 0;

            return new ComparativoResponse
            {
                Actual = totalHoy,
                Anterior = totalAyer,
                Porcentaje = totalAyer == 0
                    ? 100
                    : Math.Round(((totalHoy - totalAyer) / totalAyer) * 100, 2)
            };
        }

        public async Task<ComparativoResponse> GetComparativoMensual(int establishmentId, DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var finMesAnterior = inicioMes.AddDays(-1);

            var totalMesActual = await _context.Ventas
                .Where(x => x.FechaEmision >= inicioMes && x.FechaEmision <= fecha && x.EstablishmentId == establishmentId)
                .SumAsync(x => (decimal?)x.Total) ?? 0;

            var totalMesAnterior = await _context.Ventas
                .Where(x => x.FechaEmision >= inicioMesAnterior && x.FechaEmision <= finMesAnterior && x.EstablishmentId == establishmentId)
                .SumAsync(x => (decimal?)x.Total) ?? 0;

            return new ComparativoResponse
            {
                Actual = totalMesActual,
                Anterior = totalMesAnterior,
                Porcentaje = totalMesAnterior == 0
                    ? 100
                    : Math.Round(((totalMesActual - totalMesAnterior) / totalMesAnterior) * 100, 2)
            };
        }

        public async Task<List<ProductividadEmpleadoDto>> GetProductividadPersonal(
    int establishmentId,
    DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var data = await _context.ventaEmpleados
                .Include(v => v.Empleado)
                .Include(v => v.Venta)
                    .ThenInclude(v => v.Detalles)
                .Where(v =>
                    v.Venta.EstablishmentId == establishmentId &&
                    v.FechaRegistro >= inicioMes &&
                    v.FechaRegistro < finMes &&
                    !v.Venta.IsAnnulled
                )
                .ToListAsync();

            var productividad = data
                .GroupBy(v => v.Empleado)
                .Select(g => new ProductividadEmpleadoDto
                {
                    Empleado = g.Key.FirstName + " " + g.Key.LastName,
                    Cantidad = g.Count(), // número de servicios
                    Importe = g.Sum(x =>
                        x.Venta.Detalles
                            //.Where(d => d.Codigo == x. .productDefinition.Code)
                            .Sum(d => d.PrecioUnitario)
                    )
                })
                .OrderByDescending(x => x.Importe)
                .ToList();

            return productividad;
        }

        public async Task<List<ContribucionEstilistaDto>> GetContribucionEstilistaDia(
    int establishmentId,
    DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var data = await _context.ventaEmpleados
                .Include(v => v.productDefinition)
                .Include(v => v.Empleado)
                .Include(v => v.Venta)
                    .ThenInclude(v => v.Detalles)
                .Where(v =>
                    v.Venta.EstablishmentId == establishmentId &&
                    v.Venta.FechaEmision >= inicioDia &&
                    v.Venta.FechaEmision < finDia &&
                    !v.Venta.IsAnnulled
                )
                .ToListAsync();

            var result = data
                .GroupBy(v => v.Empleado)
                .Select(g => new ContribucionEstilistaDto
                {
                    Estilista = g.Key.FirstName + " " + g.Key.LastName,

                    Importe = g.Sum(ve =>
                        ve.Venta.Detalles
                            .Where(d => d.Codigo== ve.productDefinition.Code)
                            .Sum(d => d.PrecioUnitario * d.Cantidad)
                    )
                })
                .ToList();

            return result;
        }


        public async Task<List<ContribucionEstilistaDto>> GetContribucionEstilistaMes(
    int establishmentId,
    DateTime fecha)
        {
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var data = await _context.ventaEmpleados
                .Include(v => v.Empleado)
                .Include(v => v.Venta)
                    .ThenInclude(v => v.Detalles)
                .Where(v =>
                    v.Venta.EstablishmentId == establishmentId &&
                    v.Venta.FechaEmision >= inicioMes &&
                    v.Venta.FechaEmision < finMes &&
                    !v.Venta.IsAnnulled
                )
                .ToListAsync(); // 👈 MATERIALIZAR

            var result = data
                .GroupBy(v => v.Empleado)
                .Select(g => new ContribucionEstilistaDto
                {
                    Estilista = g.Key.FirstName + " " + g.Key.LastName,
                    Importe = g.Sum(x =>
                        x.Venta.Detalles
                            //.Where(d => d.ProductDefinitionId == x.ProductDefinitionId)
                            .Sum(d => d.PrecioUnitario * d.Cantidad)
                    )
                })
                .ToList();

            return result;
        }


    }


}
