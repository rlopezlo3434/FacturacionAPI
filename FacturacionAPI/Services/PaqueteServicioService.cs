using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace FacturacionAPI.Services
{
    public class PaqueteServicioService
    {
        private readonly SistemaVentasDbContext _context;

        public PaqueteServicioService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicePackageDto>> GetAllAsync()
        {
            return await _context.ServicePackage
                .Select(x => new ServicePackageDto
                {
                    Id = x.Id,
                    Description = x.Description,
                    IsActive = x.IsActive,

                    Items = x.Items.Select(i => new ServicePackageItemDto
                    {
                        Id = i.Id,
                        ItemType = i.ItemType,
                        ProductId = i.ProductId,
                        ServiceMasterId = i.ServiceMasterId,
                        Quantity = i.Quantity,
                        ServicePackageId = i.ServicePackageId

                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<ServicePackageDto?> GetByIdAsync(int id)
        {
            return await _context.ServicePackage
                .Where(x => x.Id == id)
                .Select(x => new ServicePackageDto
                {
                    Id = x.Id,
                    Description = x.Description,
                    IsActive = x.IsActive,

                    Items = x.Items.Select(i => new ServicePackageItemDto
                    {
                        Id = i.Id,
                        ItemType = i.ItemType,
                        ProductId = i.ProductId,
                        ServiceMasterId = i.ServiceMasterId,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> CreateAsync(ServicePackageDto dto)
        {
            var package = new ServicePackage
            {
                Description = dto.Description,
                IsActive = true
            };

            await _context.ServicePackage.AddAsync(package);
            await _context.SaveChangesAsync();

            if (dto.Items != null && dto.Items.Any())
            {
                var items = dto.Items.Select(x => new ServicePackageItem
                {
                    ServicePackageId = package.Id,
                    ItemType = x.ItemType,
                    ProductId = x.ProductId,
                    ServiceMasterId = x.ServiceMasterId,
                    Quantity = x.Quantity
                });

                await _context.ServicePackageItem.AddRangeAsync(items);
            }

            await _context.SaveChangesAsync();

            return (true, "Paquete creado correctamente.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, ServicePackageDto dto)
        {
            var package = await _context.ServicePackage
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (package == null)
                return (false, "Paquete no encontrado.");

            package.Description = dto.Description;
            package.IsActive = dto.IsActive;

            // eliminar items actuales
            _context.ServicePackageItem.RemoveRange(package.Items);

            if (dto.Items != null && dto.Items.Any())
            {
                var nuevos = dto.Items.Select(x => new ServicePackageItem
                {
                    ServicePackageId = id,
                    ItemType = x.ItemType,
                    ProductId = x.ProductId,
                    ServiceMasterId = x.ServiceMasterId,
                    Quantity = x.Quantity
                });

                await _context.ServicePackageItem.AddRangeAsync(nuevos);
            }

            await _context.SaveChangesAsync();

            return (true, "Paquete actualizado correctamente.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var package = await _context.ServicePackage.FindAsync(id);

            if (package == null)
                return (false, "Paquete no encontrado.");

            package.IsActive = false;

            await _context.SaveChangesAsync();

            return (true, "Paquete desactivado correctamente.");
        }
    }
}
