using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FacturacionAPI.Services
{
    public class KardexService
    {
        private readonly SistemaVentasDbContext _context;

        public KardexService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetProductsByEstablishment(int establishmentId)
        {
            return await _context.Items
                .Include(e => e.ProductDefinition)
                .Include(e => e.Stock)
                .Where(e => e.EstablishmentId == establishmentId && e.ProductDefinition.Item == Models.Enums.ItemEnum.producto)
                .Select(e => new
                {
                    ItemId = e.Id,
                    Code = e.ProductDefinition.Code,
                    Description = e.ProductDefinition.Description,
                    Value = e.Value,
                    Quantity = e.Stock != null ? e.Stock.Quantity : 0,
                    MinStock = e.Stock != null ? e.Stock.MinStock : 0,
                    CreatedAt = e.CreatedAt.ToString("dd/MM/yyyy"),
                    IsActive = e.IsActive
                })
        .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetStockMovementsByItem(int itemId)
        {
            return await _context.StockMovement
                .Include(m => m.Item)
                    .ThenInclude(i => i.ProductDefinition)
                .Where(m => m.ItemId == itemId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    MovementId = m.Id,
                    ProductCode = m.Item.ProductDefinition.Code,
                    ProductName = m.Item.ProductDefinition.Description,
                    MovementType = m.MovementType.ToString(),
                    Quantity = m.Quantity,
                    Notes = m.Notes,
                    Date = m.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> PostMovement(StockMovementDto move)
        {
            var stock = await _context.Stock.FirstOrDefaultAsync(s => s.ItemId == move.ItemId);
            if (stock == null)
            {
                stock = new Stock { ItemId = move.ItemId, Quantity = 0 };
                _context.Stock.Add(stock);
            }

            if (move.MovementType == MovementType.Salida)
            {
                if (stock.Quantity < move.Quantity)
                {
                    return (false, "Stock insuficiente para realizar la salida.");
                }

                stock.Quantity -= move.Quantity;
            }
            else if (move.MovementType == MovementType.Entrada)
            {
                stock.Quantity += move.Quantity;
            }
            else
            {
                return (false, "Tipo de movimiento no válido.");
            }

            var movementEntity = new StockMovement
            {
                ItemId = move.ItemId,
                MovementType = move.MovementType,
                Quantity = move.Quantity,
                Notes = move.Notes
            };

            _context.StockMovement.Add(movementEntity);
            await _context.SaveChangesAsync();

            return (true, "Movimiento registrado correctamente.");
        }

    }
}
