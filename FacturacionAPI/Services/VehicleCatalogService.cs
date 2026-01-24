using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace FacturacionAPI.Services
{
    public class VehicleCatalogService
    {
        private readonly SistemaVentasDbContext _context;
        public VehicleCatalogService(SistemaVentasDbContext context) => _context = context;

        public async Task<List<BrandDto>> GetBrandsAsync()
        {
            return await _context.VehicleBrands
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new BrandDto { Id = x.Id, Name = x.Name, isActive = x.IsActive })
                .ToListAsync();
        }

        public async Task<List<ModelDto>> GetModelsByBrandAsync(int brandId)
        {
            return await _context.VehicleModels
                .Where(x => x.IsActive && x.BrandId == brandId)
                .OrderBy(x => x.Name)
                .Select(x => new ModelDto { Id = x.Id, BrandId = x.BrandId, Name = x.Name, isActive = x.IsActive })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateBrandAsync(string name)
        {
            name = name.Trim();

            var exists = await _context.VehicleBrands.AnyAsync(x => x.Name == name);
            if (exists) return (false, "La marca ya existe.");

            _context.VehicleBrands.Add(new VehicleBrand { Name = name });
            await _context.SaveChangesAsync();
            return (true, "Marca creada correctamente.");
        }

        public async Task<(bool Success, string Message)> CreateModelAsync(int brandId, string name, bool isActive)
        {
            name = name.Trim();

            var brandExists = await _context.VehicleBrands.AnyAsync(x => x.Id == brandId);
            if (!brandExists) return (false, "La marca no existe.");

            var exists = await _context.VehicleModels.AnyAsync(x => x.BrandId == brandId && x.Name == name);
            if (exists) return (false, "El modelo ya existe para esa marca.");

            _context.VehicleModels.Add(new VehicleModel
            {
                BrandId = brandId,
                Name = name,
                IsActive = isActive
            });

            await _context.SaveChangesAsync();
            return (true, "Modelo creado correctamente.");
        }
    }
}
