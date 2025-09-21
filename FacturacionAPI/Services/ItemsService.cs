using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class ItemsService
    {
        private readonly SistemaVentasDbContext _context;

        public ItemsService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<Items> CreateItemAsync(CreateItemDto dto)
        {
            var item = new Items
            {
                Item = dto.Item,
                Description = dto.Description,
                EstablishmentId = dto.EstablishmentId,
                IsActive = true
            };

            // Si es producto, inicializamos stock
            if (dto.Item == ItemEnum.producto)
            {
                item.Stock = new Stock
                {
                    Quantity = dto.InitialQuantity,
                    MinStock = dto.MinStock
                };
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<IEnumerable<ItemsDto>> getItemsByEstablishment(int establishmentId)
        {
            return await _context.Items
                .Include(e => e.Establishment)
                .Where(e => e.EstablishmentId == establishmentId)
                .Select(e => new ItemsDto
                {
                    Id = e.Id,
                    Item = e.Item.ToString() == "servicio" ? "Servicio" : "Producto",
                    Description = e.Description,
                    CreatedAt = e.CreatedAt.ToString("dd/MM/yyyy"),
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }
    }
}
