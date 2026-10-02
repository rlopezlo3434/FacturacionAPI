using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
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
                PrecioCosto = d.PrecioCosto,
                PrecioCompra = d.PrecioCompra
            }).ToList();

            // El precio de costo es el que determina lo que realmente se paga al proveedor
            decimal montoIngresado = detalles.Sum(d => d.PrecioCosto * d.Cantidad);

            decimal subtotal, igv, total;
            if (dto.ConIgv)
            {
                // Los precios ingresados ya incluyen IGV → se descompone para obtener subtotal e IGV
                total = montoIngresado;
                subtotal = Math.Round(total / 1.18m, 2);
                igv = Math.Round(total - subtotal, 2);
            }
            else
            {
                // Los precios ingresados son sin IGV → se le agrega el IGV para obtener el total
                subtotal = montoIngresado;
                igv = Math.Round(subtotal * 0.18m, 2);
                total = Math.Round(subtotal + igv, 2);
            }

            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                Serie = dto.Serie,
                Correlativo = dto.Correlativo,
                FechaDocumento = dto.FechaDocumento,
                FechaCreacion = DateTime.UtcNow,
                Moneda = dto.Moneda,
                ConIgv = dto.ConIgv,
                Subtotal = subtotal,
                Igv = igv,
                Total = total,
                Detalles = detalles
            };

            _context.Compras.Add(compra);

            // ✅ Actualiza costo, stock y kardex (entrada) de cada producto comprado
            var productIds = dto.Detalles.Select(d => d.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            bool esDolar = dto.Moneda.Equals("DOLARES", StringComparison.OrdinalIgnoreCase) ||
                           dto.Moneda.Equals("USD", StringComparison.OrdinalIgnoreCase);

            int.TryParse(dto.Correlativo, out var documentoNumero);

            foreach (var detalle in dto.Detalles)
            {
                var product = products.FirstOrDefault(p => p.Id == detalle.ProductId);
                if (product == null) continue;

                // PrecioCosto: lo que se paga al proveedor. PrecioCompra: precio de venta a actualizar.
                if (esDolar)
                    product.CostDolar = detalle.PrecioCosto;
                else
                    product.Cost = detalle.PrecioCosto;

                if (detalle.PrecioCompra > 0)
                {
                    if (esDolar)
                        product.PriceDolar = detalle.PrecioCompra;
                    else
                        product.Price = detalle.PrecioCompra;
                }

                product.Quantity += detalle.Cantidad;

                _context.ProductStockMovements.Add(new ProductStockMovement
                {
                    ProductId = product.Id,
                    MovementType = MovementType.Entrada,
                    Quantity = detalle.Cantidad,
                    DocumentoTipo = "COMPRA",
                    DocumentoSerie = dto.Serie ?? "",
                    DocumentoNumero = documentoNumero,
                    Compra = compra,
                    Notes = $"Entrada por compra {dto.Serie}-{dto.Correlativo}"
                });
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
            ConIgv = c.ConIgv,
            Detalles = c.Detalles?.Select(d => new CompraDetalleResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductNombre = d.Product?.Name ?? "",
                ProductCodigo = d.Product?.Code ?? "",
                Cantidad = d.Cantidad,
                PrecioCosto = d.PrecioCosto,
                PrecioCompra = d.PrecioCompra
            }).ToList() ?? new()
        };
    }
}
