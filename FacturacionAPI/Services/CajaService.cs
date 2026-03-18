using FacturacionAPI.Data;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System;

namespace FacturacionAPI.Services
{
    public class CajaService
    {
        private readonly SistemaVentasDbContext _context;

        public CajaService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<CajaApertura> AbrirCaja(int establishmentId, decimal montoApertura)
        {
            var cajaAbierta = await _context.CajaAperturas
                .FirstOrDefaultAsync(x => x.EstablishmentId == establishmentId && !x.Cerrada);

            if (cajaAbierta != null)
                throw new Exception("Ya existe una caja abierta.");

            var caja = new CajaApertura
            {
                EstablishmentId = establishmentId,
                MontoApertura = montoApertura,
                FechaApertura = DateTime.Now
            };

            _context.CajaAperturas.Add(caja);
            await _context.SaveChangesAsync();

            return caja;
        }

        // ------------------------------------------------
        // 2. REGISTRAR MOVIMIENTO MANUAL
        // ------------------------------------------------
        public async Task<object> RegistrarMovimiento(int cajaId, decimal monto, string tipo, string motivo)
        {
            var caja = await _context.CajaAperturas.FindAsync(cajaId);
            if (caja == null || caja.Cerrada)
                throw new Exception("Caja no válida o ya está cerrada.");

            var mov = new CajaMovimiento
            {
                CajaAperturaId = cajaId,
                Monto = monto,
                Tipo = tipo.ToUpper(), // "INGRESO" o "EGRESO"
                Motivo = motivo
            };

            _context.CajaMovimientos.Add(mov);
            await _context.SaveChangesAsync();

            return new
            {
                id = mov.Id,
                cajaAperturaId = mov.CajaAperturaId,
                ventaId = mov.VentaId,
                monto = mov.Monto,
                tipo = mov.Tipo,
                motivo = mov.Motivo
            }; 
        }

        public async Task<object?> RegistrarMovimientoPorVenta(int ventaId)
        {
            var venta = await _context.Ventas
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                throw new Exception("Venta no encontrada.");

            // 🚫 SOLO EFECTIVO genera movimiento de caja
            if (venta.MetodoPago != MetodoPago.Efectivo)
                return null;

            var caja = await _context.CajaAperturas
                .FirstOrDefaultAsync(c =>
                    c.EstablishmentId == venta.EstablishmentId &&
                    !c.Cerrada);

            if (caja == null)
                throw new Exception("No hay caja abierta en este establecimiento.");

            var movimiento = new CajaMovimiento
            {
                CajaAperturaId = caja.Id,
                VentaId = venta.Id,
                Monto = venta.Total,
                Tipo = "INGRESO",
                Motivo = $"Venta {venta.Serie}-{venta.Numero}",
                FechaRegistro = DateTime.Now
            };

            _context.CajaMovimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return new
            {
                id = movimiento.Id,
                cajaAperturaId = movimiento.CajaAperturaId,
                ventaId = movimiento.VentaId,
                monto = movimiento.Monto,
                tipo = movimiento.Tipo,
                motivo = movimiento.Motivo,
                fecha = movimiento.FechaRegistro
            };
        }

        private string ClasificarMovimiento(string metodoPago)
        {
            metodoPago = metodoPago.ToLower();

            if (metodoPago == "efectivo")
                return "INGRESO";

            // Yape, tarjeta, transferencia = egresos porque no entra efectivo real
            return "EGRESO";
        }

        // ------------------------------------------------
        // 4. OBTENER LA CAJA ABIERTA
        // ------------------------------------------------
        public async Task<CajaApertura> ObtenerCajaAbierta(int establishmentId)
        {
            return await _context.CajaAperturas
                .Include(c => c.Movimientos)
                .Include(c => c.Cierre)
                .FirstOrDefaultAsync(c => c.EstablishmentId == establishmentId && !c.Cerrada);
        }

        // ------------------------------------------------
        // 5. CERRAR CAJA
        // ------------------------------------------------
        public async Task<object> CerrarCaja(int establishmentId, int cajaAperturaId, decimal efectivoContado, string observaciones)
        {
            var caja = await _context.CajaAperturas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.EstablishmentId == establishmentId && !c.Cerrada && c.Id == cajaAperturaId);

            if (caja == null)
                throw new Exception("No existe caja abierta.");

            // Calcular efectivo esperado
            decimal ingresos = caja.Movimientos.Where(x => x.Tipo == "INGRESO").Sum(x => x.Monto);
            decimal egresos = caja.Movimientos.Where(x => x.Tipo == "EGRESO").Sum(x => x.Monto);

            decimal calculado = caja.MontoApertura + ingresos - egresos;

            var cierre = new CajaCierre
            {
                CajaAperturaId = caja.Id,
                EfectivoCalculado = calculado,
                EfectivoContado = efectivoContado,
                Observaciones = observaciones,
                FechaCierre = DateTime.Now
            };

            caja.Cerrada = true;
            caja.Cierre = cierre;

            await _context.SaveChangesAsync();

            return new
            {
                id = cierre.Id,
                cajaAperturaId = cierre.CajaAperturaId,
            }; ;
        }

        // ------------------------------------------------
        // 6. DETALLE COMPLETO DE CAJA
        // ------------------------------------------------
        public async Task<object> ObtenerDetalle(int cajaId)
        {
            var caja = await _context.CajaAperturas
                .Include(c => c.Movimientos)
                .Include(c => c.Cierre)
                .FirstOrDefaultAsync(c => c.Id == cajaId);

            if (caja == null) throw new Exception("Caja no encontrada.");


            var movimientosFiltrados = caja.Movimientos
                .Where(m => m.Venta == null || m.Venta.IsAnnulled == false)
                .ToList();

            return new
            {
                caja.Id,
                caja.MontoApertura,
                caja.FechaApertura,
                caja.Cerrada,
                Movimientos = movimientosFiltrados.Select(m => new
                {
                    m.Id,
                    m.FechaRegistro,
                    m.Tipo,
                    m.Monto,
                    m.Motivo,
                    m.VentaId
                }),
                Cierre = caja.Cierre == null ? null : new
                {
                    caja.Cierre.EfectivoCalculado,
                    caja.Cierre.EfectivoContado,
                    caja.Cierre.Diferencia,
                    caja.Cierre.Observaciones,
                    caja.Cierre.FechaCierre
                }
            };
        }

        public async Task<List<CajaApertura>> ListarCajasAbiertas(int establishmentId)
        {
            return await _context.CajaAperturas
                .Where(c => c.EstablishmentId == establishmentId)
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();
        }

        public async Task<byte[]> GenerarExcelCaja(int establishmentId, DateTime? fecha)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var dia = (fecha ?? DateTime.Today).Date;
            var inicioDia = dia;
            var finDia = dia.AddDays(1);

            var movimientos = await _context.CajaMovimientos
                .Where(m =>
                    m.FechaRegistro >= inicioDia &&
                    m.FechaRegistro < finDia &&
                    m.CajaApertura.EstablishmentId == establishmentId)
                .Include(m => m.Venta)
                .Include(m => m.CajaApertura)
                    .ThenInclude(c => c.Cierre)
                .Include(m => m.CajaApertura)
                    .ThenInclude(c => c.Establishment)
                .OrderBy(m => m.FechaRegistro)
                .ToListAsync();

            if (!movimientos.Any())
                throw new Exception("No se encontraron movimientos para ese día.");

            var caja = movimientos.First().CajaApertura;

            // ===============================
            // CALCULOS
            // ===============================

            decimal ventas = movimientos
                .Where(x => x.VentaId != null)
                .Sum(x => x.Monto);

            decimal ingresos = movimientos
                .Where(x => x.Tipo == "INGRESO")
                .Sum(x => x.Monto);

            decimal egresos = movimientos
                .Where(x => x.Tipo == "EGRESO")
                .Sum(x => x.Monto);

            decimal efectivoCalculado = caja.MontoApertura + ventas + ingresos - egresos;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Reporte Caja");

                int row = 1;

                // ===============================
                // CABECERA
                // ===============================

                ws.Cells[row, 1].Value = "REPORTE DE CAJA";
                ws.Cells[row, 1].Style.Font.Size = 16;
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1, row, 6].Merge = true;
                ws.Cells[row, 1, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row += 2;

                // ===============================
                // DATOS DE APERTURA
                // ===============================

                ws.Cells[row, 1].Value = "Caja ID:";
                ws.Cells[row, 2].Value = caja.Id;

                ws.Cells[row + 1, 1].Value = "Fecha Apertura:";
                ws.Cells[row + 1, 2].Value = caja.FechaApertura;

                ws.Cells[row + 2, 1].Value = "Monto Apertura:";
                ws.Cells[row + 2, 2].Value = caja.MontoApertura;

                ws.Cells[row + 3, 1].Value = "Estado:";
                ws.Cells[row + 3, 2].Value = caja.Cerrada ? "CERRADA" : "ABIERTA";

                row += 5;

                // ===============================
                // TABLA MOVIMIENTOS
                // ===============================

                ws.Cells[row, 1].Value = "Tipo";
                ws.Cells[row, 2].Value = "Monto";
                ws.Cells[row, 3].Value = "Fecha";
                ws.Cells[row, 4].Value = "Motivo";
                ws.Cells[row, 5].Value = "Comprobante";

                using (var range = ws.Cells[row, 1, row, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                row++;

                foreach (var m in movimientos)
                {
                    ws.Cells[row, 1].Value = m.Tipo;
                    ws.Cells[row, 2].Value = (double)m.Monto;
                    ws.Cells[row, 3].Value = m.FechaRegistro.ToString("dd-MM-yyyy HH:mm");
                    ws.Cells[row, 4].Value = m.Motivo;

                    ws.Cells[row, 5].Value = m.VentaId == null
                        ? "-"
                        : $"{m.Venta.Serie}-{m.Venta.Numero}";

                    row++;
                }

                row += 2;

                // ===============================
                // RESUMEN
                // ===============================

                ws.Cells[row, 1].Value = "VENTAS:";
                ws.Cells[row, 2].Value = (double)ventas;

                ws.Cells[row + 1, 1].Value = "INGRESOS:";
                ws.Cells[row + 1, 2].Value = (double)ingresos;

                ws.Cells[row + 2, 1].Value = "EGRESOS:";
                ws.Cells[row + 2, 2].Value = (double)egresos;

                ws.Cells[row + 3, 1].Value = "EFECTIVO CALCULADO:";
                ws.Cells[row + 3, 2].Value = (double)caja.MontoApertura + (double)ingresos - (double)egresos;

                //if (caja.Cierre != null)
                //{
                //    ws.Cells[row + 4, 1].Value = "EFECTIVO CONTADO:";
                //    ws.Cells[row + 4, 2].Value = (double)caja.Cierre.EfectivoContado;

                //    ws.Cells[row + 5, 1].Value = "DIFERENCIA:";
                //    ws.Cells[row + 5, 2].Value = (double)caja.Cierre.Diferencia;

                //    ws.Cells[row + 6, 1].Value = "Observaciones:";
                //    ws.Cells[row + 6, 2].Value = caja.Cierre.Observaciones;
                //}

                ws.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        public async Task<byte[]> GenerarReporteMensual(
    int establecimientoId,
    int year,
    int month)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var inicioMes = new DateTime(year, month, 1);
            var finMes = inicioMes.AddMonths(1);

            // =====================================
            // CAJAS CON MOVIMIENTOS DEL MES
            // =====================================
            var aperturas = await _context.CajaAperturas
                .Include(c => c.Movimientos
                    .Where(m =>
                        m.FechaRegistro >= inicioMes &&
                        m.FechaRegistro < finMes &&
                        (m.Venta == null || !m.Venta.IsAnnulled)))
                    .ThenInclude(m => m.Venta)
                .Where(c => c.EstablishmentId == establecimientoId)
                .ToListAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Caja Mensual");

            int row = 1;

            ws.Cells[row++, 1].Value =
                $"REPORTE MENSUAL DE CAJA - {inicioMes:MMMM yyyy}";
            row++;

            // =============================
            // CABECERA
            // =============================
            ws.Cells[row, 1].Value = "Fecha";
            ws.Cells[row, 2].Value = "Tipo";
            ws.Cells[row, 3].Value = "Monto";
            ws.Cells[row, 4].Value = "Motivo";

            ws.Cells[row, 1, row, 4].Style.Font.Bold = true;

            row++;

            decimal totalIngresos = 0;
            decimal totalEgresos = 0;

            // =====================================
            // MOVIMIENTOS
            // =====================================
            var movimientos = aperturas
                .SelectMany(a => a.Movimientos)
                .OrderBy(m => m.FechaRegistro)
                .ToList();

            foreach (var mov in movimientos)
            {
                ws.Cells[row, 1].Value =
                    mov.FechaRegistro.ToString("yyyy-MM-dd HH:mm");

                ws.Cells[row, 2].Value = mov.Tipo;
                ws.Cells[row, 3].Value = mov.Monto;
                ws.Cells[row, 4].Value = mov.Motivo;

                if (mov.Tipo == "INGRESO")
                    totalIngresos += mov.Monto;
                else
                    totalEgresos += mov.Monto;

                row++;
            }

            // =====================================
            // TOTALES
            // =====================================
            row++;

            ws.Cells[row, 2].Value = "TOTAL INGRESOS:";
            ws.Cells[row, 3].Value = totalIngresos;

            row++;

            ws.Cells[row, 2].Value = "TOTAL EGRESOS:";
            ws.Cells[row, 3].Value = totalEgresos;

            row++;

            ws.Cells[row, 2].Value = "SALDO TOTAL:";
            ws.Cells[row, 3].Value =
                totalIngresos - totalEgresos;

            ws.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

    }
}
