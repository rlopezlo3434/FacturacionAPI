using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;

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
    }
}
