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

        public async Task<(bool Success, string Message)> UpdateItemAsync(CreateItemDto dto)
        {
            // 1️⃣ Buscar el item por su Id
            var existingItem = await _context.Items
                .Include(i => i.ProductDefinition) // para acceder al nombre
                .FirstOrDefaultAsync(i => i.ProductDefinition.Code == dto.Code);

            if (existingItem == null)
                return (false, "El item no existe.");

            // 2️⃣ Actualizar el nombre en ProductDefinition (si cambió)
            if (!string.IsNullOrWhiteSpace(dto.Description) &&
                existingItem.ProductDefinition.Description != dto.Description)
            {
                existingItem.ProductDefinition.Description = dto.Description;
                existingItem.ProductDefinition.UpdatedAt = DateTime.Now;
            }

            // 3️⃣ Actualizar el precio (Value)
            if (existingItem.Value != dto.Value)
            {
                existingItem.Value = dto.Value;
                existingItem.UpdatedAt = DateTime.Now;
            }

            // 4️⃣ Guardar cambios
            await _context.SaveChangesAsync();

            return (true, "Item actualizado correctamente.");
        }

        public async Task<(bool Success, string Message)> CreateItemAsync(CreateItemDto dto)
        {
            // 1️⃣ Verificar si ya existe la definición global
            var existingDefinition = await _context.ProductDefinition
                .FirstOrDefaultAsync(p => p.Code == dto.Code);

            ProductDefinition productDefinition;

            if (existingDefinition == null)
            {
                // 2️⃣ Crear nueva definición global si no existe
                productDefinition = new ProductDefinition
                {
                    Code = string.IsNullOrWhiteSpace(dto.Code)
                            ? GenerateCode(dto.Description)  // genera si es null, vacío o solo espacios
                            : dto.Code,
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
                    Value = e.Value,
                    Description = e.ProductDefinition.Description,
                    CreatedAt = e.CreatedAt.ToString("dd/MM/yyyy"),
                    IsActive = e.IsActive,
                    Code = e.ProductDefinition.Code
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> UpdateItemStateAsync(int id, bool estatus)
        {
            var item = await _context.Items.FirstOrDefaultAsync(e => e.Id == id);

            if (item == null)
                return (false, "Item no encontrado");

            item.IsActive = estatus;

            _context.Items.Update(item);

            await _context.SaveChangesAsync();

            return (true, "Item actualizado correctamente");
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
