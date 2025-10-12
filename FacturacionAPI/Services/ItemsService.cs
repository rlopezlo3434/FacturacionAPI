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

        public async Task<(bool Success, string Message)> CreateItemAsync(CreateItemDto dto)
        {
            // 1️⃣ Verificar si ya existe la definición global
            var existingDefinition = await _context.ProductDefinition
                .FirstOrDefaultAsync(p => p.Code == dto.Codigo);

            ProductDefinition productDefinition;

            if (existingDefinition == null)
            {
                // 2️⃣ Crear nueva definición global si no existe
                productDefinition = new ProductDefinition
                {
                    Code = string.IsNullOrWhiteSpace(dto.Codigo)
                            ? GenerateCode(dto.Description)  // genera si es null, vacío o solo espacios
                            : dto.Codigo,
                    Item = dto.Item,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.ProductDefinition.Add(productDefinition);
                await _context.SaveChangesAsync();
            }
            else
            {
                productDefinition = existingDefinition;
            }

            // 3️⃣ Verificar si ya existe el item para esa tienda
            var existingItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ProductDefinitionId == productDefinition.Id && i.EstablishmentId == dto.EstablishmentId);

            if (existingItem != null)
                return (false, "El producto o servicio ya está registrado para esta tienda.");

            // 4️⃣ Crear el Item (asociado a la tienda)
            var newItem = new Item
            {
                ProductDefinitionId = productDefinition.Id,
                EstablishmentId = dto.EstablishmentId,
                Value = dto.Value,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();

            // 5️⃣ Si es un producto, crear su stock inicial
            if (dto.Item == ItemEnum.producto)
            {
                var stock = new Stock
                {
                    ItemId = newItem.Id,
                    Quantity = dto.InitialQuantity,
                    MinStock = dto.MinStock,
                    CreatedAt = DateTime.Now
                };
                _context.Stock.Add(stock);
                await _context.SaveChangesAsync();
            }

            return (true, "Item creado correctamente.");
        }

        public async Task<IEnumerable<ItemsDto>> getItemsByEstablishment(int establishmentId)
        {
            return await _context.Items
                .Include(e => e.ProductDefinition)
                .Include(e => e.Stock)
                .Where(e => e.EstablishmentId == establishmentId)
                .Select(e => new ItemsDto
                {
                    Id = e.Id,
                    Item = e.ProductDefinition.Item.ToString() == "servicio" ? "Servicio" : "Producto",
                    Description = e.ProductDefinition.Description,
                    CreatedAt = e.CreatedAt.ToString("dd/MM/yyyy"),
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public string GenerateCode(string description)
        {
            var prefix = description.Length >= 4 ? description[..4].ToUpper() : description.ToUpper();
            var random = new Random();
            var number = random.Next(1000, 9999);
            return $"{prefix}-{number}";
        }
    }
}
