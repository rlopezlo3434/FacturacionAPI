using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class VehicleService
    {
        private readonly SistemaVentasDbContext _context;
        public VehicleService(SistemaVentasDbContext context) => _context = context;

        public async Task<List<VehicleListDto>> GetVehiclesAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Model)
                .ThenInclude(m => m.Brand)
                .Include(v => v.Owners)
                .ThenInclude(o => o.Client)
                .Select(v => new VehicleListDto
                {
                    Id = v.Id,
                    Plate = v.Plate,
                    SerialNumber = v.SerialNumber,
                    Vin = v.Vin,
                    Year = v.Year,
                    Color = v.Color,
                    CurrentMileageKm = v.CurrentMileageKm,
                    IsActive = v.IsActive,

                    Brand = new CatalogItemDto
                    {
                        Id = v.Model.Brand.Id,
                        Name = v.Model.Brand.Name
                    },
                    Model = new CatalogItemDto
                    {
                        Id = v.Model.Id,
                        Name = v.Model.Name
                    },
                    Owner = v.Owners
                        .Where(o => o.IsCurrentOwner)
                        .Select(o => new CatalogItemDto
                        {
                            Id = o.Client.Id,
                            Name = o.Client.Names
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateVehicleAsync(VehicleCreateDto dto)
        {
            var exists = await _context.Vehicles.AnyAsync(v => v.Plate == dto.Plate);
            if (exists)
                return (false, "Ya existe un vehículo con esa placa.");

            var model = await _context.VehicleModels.FirstOrDefaultAsync(m => m.Id == dto.ModelId);
            if (model == null)
                return (false, "Modelo no válido.");

            if (model.BrandId != dto.BrandId)
                return (false, "El modelo no pertenece a la marca seleccionada.");

            var client = await _context.Client.FirstOrDefaultAsync(c => c.Id == dto.OwnerClientId);
            if (client == null)
                return (false, "Cliente dueño no válido.");

            var vehicle = new Vehicle
            {
                Plate = dto.Plate.Trim().ToUpper(),
                SerialNumber = dto.SerialNumber.Trim(),
                Vin = dto.Vin.Trim(),
                ModelId = dto.ModelId,
                Year = dto.Year,
                Color = dto.Color,
                CurrentMileageKm = dto.CurrentMileageKm,
                IsActive = true
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var owner = new VehicleOwner
            {
                VehicleId = vehicle.Id,
                ClientId = dto.OwnerClientId,
                StartDate = DateTime.Now,
                IsCurrentOwner = true
            };

            _context.VehicleOwners.Add(owner);
            await _context.SaveChangesAsync();

            return (true, "Vehículo creado correctamente.");
        }

        public async Task<(bool Success, string Message)> UpdateVehicleAsync(int id, VehicleUpdateDto dto)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Owners)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return (false, "No se encontró el vehículo.");

            var plateExists = await _context.Vehicles.AnyAsync(v => v.Plate == dto.Plate && v.Id != id);
            if (plateExists)
                return (false, "Ya existe otro vehículo con esa placa.");

            var model = await _context.VehicleModels.FirstOrDefaultAsync(m => m.Id == dto.ModelId);
            if (model == null)
                return (false, "Modelo no válido.");

            if (model.BrandId != dto.BrandId)
                return (false, "El modelo no pertenece a la marca seleccionada.");

            vehicle.Plate = dto.Plate.Trim().ToUpper();
            vehicle.SerialNumber = dto.SerialNumber.Trim();
            vehicle.Vin = dto.Vin.Trim();
            vehicle.BrandId = dto.BrandId;
            vehicle.ModelId = dto.ModelId;
            vehicle.Year = dto.Year;
            vehicle.Color = dto.Color;
            vehicle.CurrentMileageKm = dto.CurrentMileageKm;
            vehicle.IsActive = dto.IsActive;

            var currentOwner = vehicle.Owners.FirstOrDefault(o => o.IsCurrentOwner);

            if (currentOwner == null || currentOwner.ClientId != dto.OwnerClientId)
            {
                if (currentOwner != null)
                {
                    currentOwner.IsCurrentOwner = false;
                    currentOwner.EndDate = DateTime.Now;
                }

                var newOwner = new VehicleOwner
                {
                    VehicleId = vehicle.Id,
                    ClientId = dto.OwnerClientId,
                    StartDate = DateTime.Now,
                    IsCurrentOwner = true
                };

                _context.VehicleOwners.Add(newOwner);
            }

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return (true, "Vehículo actualizado correctamente.");
        }
    }
}
