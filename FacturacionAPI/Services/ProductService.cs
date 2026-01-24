using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class ProductService
    {
        private readonly SistemaVentasDbContext _context;

        public ProductService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductListDto>> GetProductsAsync()
        {
            return await _context.Products
                .Include(x => x.Brand)
                .OrderByDescending(x => x.Id)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Quantity = x.Quantity,
                    SerialCode = x.SerialCode,
                    Price = x.Price, // ✅
                    IsMultiBrand = x.IsMultiBrand,
                    Brand = x.Brand == null ? null : new CatalogItemDto
                    {
                        Id = x.Brand.Id,
                        Name = x.Brand.Name
                    }
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateProductAsync(ProductCreateUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "El nombre del producto es obligatorio.");

            if (dto.Quantity < 0)
                return (false, "La cantidad no puede ser negativa.");

            if (dto.Price < 0)
                return (false, "El precio no puede ser negativo.");

            if (dto.IsMultiBrand)
                dto.BrandId = null;

            if (!dto.IsMultiBrand && dto.BrandId == null)
                return (false, "La marca es obligatoria si el producto no es MultiMarca.");

            if (dto.BrandId != null)
            {
                var brandExists = await _context.VehicleBrands.AnyAsync(x => x.Id == dto.BrandId);
                if (!brandExists)
                    return (false, "La marca seleccionada no existe.");
            }

            var code = await GenerateCodeAsync();

            var product = new Product
            {
                Code = code,
                Name = dto.Name.Trim(),
                Quantity = dto.Quantity,
                SerialCode = dto.SerialCode?.Trim(),
                Price = dto.Price, // ✅
                IsMultiBrand = dto.IsMultiBrand,
                BrandId = dto.IsMultiBrand ? null : dto.BrandId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return (true, "Producto creado correctamente.");
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(int id, ProductCreateUpdateDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return (false, "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "El nombre del producto es obligatorio.");

            if (dto.Quantity < 0)
                return (false, "La cantidad no puede ser negativa.");

            if (dto.Price < 0)
                return (false, "El precio no puede ser negativo.");

            if (dto.IsMultiBrand)
                dto.BrandId = null;

            if (!dto.IsMultiBrand && dto.BrandId == null)
                return (false, "La marca es obligatoria si el producto no es MultiMarca.");

            if (dto.BrandId != null)
            {
                var brandExists = await _context.VehicleBrands.AnyAsync(x => x.Id == dto.BrandId);
                if (!brandExists)
                    return (false, "La marca seleccionada no existe.");
            }

            product.Name = dto.Name.Trim();
            product.Quantity = dto.Quantity;
            product.SerialCode = dto.SerialCode?.Trim();
            product.Price = dto.Price; 
            product.IsMultiBrand = dto.IsMultiBrand;
            product.BrandId = dto.IsMultiBrand ? null : dto.BrandId;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return (true, "Producto actualizado correctamente.");
        }

        private async Task<string> GenerateCodeAsync()
        {
            var lastCode = await _context.Products
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastCode))
                return "PR0001";

            var numberPart = lastCode.Replace("PR", "");
            if (!int.TryParse(numberPart, out int lastNumber))
                return "PR0001";

            var newNumber = lastNumber + 1;
            return $"PR{newNumber:D4}";
        }
    }
}
