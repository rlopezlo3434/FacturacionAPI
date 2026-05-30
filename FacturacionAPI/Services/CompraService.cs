using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class CompraService
    {
        private readonly SistemaVentasDbContext _context;

        public CompraService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompraResponseDto>> GetAllAsync()
        {
            return await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles).ThenInclude(d => d.Product)
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<CompraResponseDto?> GetByIdAsync(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            return compra == null ? null : MapToDto(compra);
        }

        public async Task<CompraResponseDto> CreateAsync(CompraCreateDto dto)
        {
            var detalles = dto.Detalles.Select(d => new CompraDetalle
            {
                ProductId = d.ProductId,
                Cantidad = d.Cantidad,
                PrecioCompra = d.PrecioCompra
            }).ToList();

            // PrecioCompra ya incluye IGV → dividir para obtener subtotal e IGV
            decimal total = detalles.Sum(d => d.PrecioCompra * d.Cantidad);
            //decimal subtotal = Math.Round(total / 1.18m, 2);
            //decimal igv = Math.Round(total - subtotal, 2);

            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                Serie = dto.Serie,
                Correlativo = dto.Correlativo,
                FechaDocumento = dto.FechaDocumento,
                FechaCreacion = DateTime.UtcNow,
                Moneda = dto.Moneda,
                //Subtotal = subtotal,
                //Igv = igv,
                Total = total,
                Detalles = detalles
            };

            _context.Compras.Add(compra);

            if (dto.Moneda.Equals("DOLARES", StringComparison.OrdinalIgnoreCase) ||
                dto.Moneda.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                var productIds = dto.Detalles.Select(d => d.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var detalle in dto.Detalles)
                {
                    var product = products.FirstOrDefault(p => p.Id == detalle.ProductId);
                    if (product != null)
                        product.CostDolar = detalle.PrecioCompra;
                }
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(compra.Id) ?? MapToDto(compra);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null) return false;

            _context.CompraDetalles.RemoveRange(compra.Detalles);
            _context.Compras.Remove(compra);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CompraResponseDto MapToDto(Compra c) => new()
        {
            Id = c.Id,
            ProveedorId = c.ProveedorId,
            ProveedorRazonSocial = c.Proveedor?.RazonSocial ?? "",
            ProveedorRuc = c.Proveedor?.Ruc ?? "",
            Serie = c.Serie,
            Correlativo = c.Correlativo,
            FechaDocumento = c.FechaDocumento,
            FechaCreacion = c.FechaCreacion,
            Subtotal = c.Subtotal,
            Igv = c.Igv,
            Total = c.Total,
            Moneda = c.Moneda,
            Detalles = c.Detalles?.Select(d => new CompraDetalleResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductNombre = d.Product?.Name ?? "",
                ProductCodigo = d.Product?.Code ?? "",
                Cantidad = d.Cantidad,
                PrecioCompra = d.PrecioCompra
            }).ToList() ?? new()
        };
    }
}
