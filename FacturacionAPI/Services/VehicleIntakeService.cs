using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class VehicleIntakeService
    {
        private readonly SistemaVentasDbContext _context;

        public VehicleIntakeService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryMasterDto>> GetInventoryMasterAsync()
        {
            return await _context.InventoryMasterItems
                .Where(x => x.IsActive)
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .Select(x => new InventoryMasterDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Group = (int)x.Group
                })
                .ToListAsync();
        }

        public async Task<List<VehicleIntakeListDto>> GetIntakesAsync()
        {
            var result = await _context.VehicleIntakes
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(x => x.Client)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new VehicleIntakeListDto
                {
                    Id = x.Id,
                    Mode = (int)x.Mode,
                    PickupAddress = x.PickupAddress,
                    MileageKm = x.MileageKm,
                    CreatedAt = x.CreatedAt,
                    IsActive = true, // ✅ cuando tengas campo real: x.IsActive

                    Vehicle = new VehicleIntakeVehicleDto
                    {
                        Id = x.Vehicle.Id,
                        Plate = x.Vehicle.Plate,
                        Brand = new CatalogItemDto
                        {
                            Id = x.Vehicle.Brand.Id,
                            Name = x.Vehicle.Brand.Name
                        },
                        Model = new CatalogItemDto
                        {
                            Id = x.Vehicle.Model.Id,
                            Name = x.Vehicle.Model.Name
                        }
                    },

                    Client = new VehicleIntakeClientDto
                    {
                        Id = x.Client.Id,
                        Names = x.Client.Names
                    }
                })
                .ToListAsync();

            return result;
        }

        public async Task<(bool Success, string Message)> CreateVehicleIntakeAsync(CreateVehicleIntakeDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == dto.VehicleId);
            if (vehicle == null) return (false, "Vehículo no válido.");

            var client = await _context.Client.FirstOrDefaultAsync(x => x.Id == dto.ClientId);
            if (client == null) return (false, "Cliente no válido.");

            var modeEnum = (IntakeModeEnum)dto.Mode;

            if (modeEnum == IntakeModeEnum.RecojoDomicilio && string.IsNullOrWhiteSpace(dto.PickupAddress))
                return (false, "La dirección de recojo es obligatoria para recojo a domicilio.");

            if (dto.MileageKm <= 0)
                return (false, "El kilometraje debe ser mayor a 0.");

            // ✅ crear cabecera
            var intake = new VehicleIntake
            {
                VehicleId = dto.VehicleId,
                ClientId = dto.ClientId,
                Mode = modeEnum,
                PickupAddress = dto.PickupAddress,
                MileageKm = dto.MileageKm,
                Observations = dto.Observations,
                CreatedAt = DateTime.Now
            };

            _context.VehicleIntakes.Add(intake);
            await _context.SaveChangesAsync();

            // ✅ guardar detalle historico
            var details = dto.InventoryItems.Select(x => new VehicleIntakeInventoryItem
            {
                VehicleIntakeId = intake.Id,
                InventoryMasterItemId = x.InventoryMasterItemId,
                IsPresent = x.IsPresent
            }).ToList();

            _context.VehicleIntakeInventoryItems.AddRange(details);
            await _context.SaveChangesAsync();

            return (true, "Internamiento registrado correctamente.");
        }

        public async Task<VehicleIntakeDetailDto?> GetIntakeDetailAsync(int id)
        {
            var intake = await _context.VehicleIntakes
                .Include(x => x.Vehicle).ThenInclude(v => v.Brand)
                .Include(x => x.Vehicle).ThenInclude(v => v.Model)
                .Include(x => x.Client)
                .Include(x => x.InventoryItems)
                    .ThenInclude(d => d.InventoryMasterItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (intake == null) return null;

            return new VehicleIntakeDetailDto
            {
                Id = intake.Id,
                Mode = (int)intake.Mode,
                PickupAddress = intake.PickupAddress,
                MileageKm = intake.MileageKm,
                Observations = intake.Observations,
                CreatedAt = intake.CreatedAt,

                Vehicle = new VehicleMiniDto2
                {
                    Id = intake.Vehicle.Id,
                    Plate = intake.Vehicle.Plate,
                    Brand = new CatalogMiniDto2
                    {
                        Id = intake.Vehicle.Brand.Id,
                        Name = intake.Vehicle.Brand.Name
                    },
                    Model = new CatalogMiniDto2
                    {
                        Id = intake.Vehicle.Model.Id,
                        Name = intake.Vehicle.Model.Name
                    }
                },

                Client = new ClientMiniDto2
                {
                    Id = intake.Client.Id,
                    Names = intake.Client.Names
                },

                InventoryItems = intake.InventoryItems
                    .OrderBy(x => x.InventoryMasterItem.Group)
                    .ThenBy(x => x.InventoryMasterItem.Name)
                    .Select(x => new VehicleIntakeInventoryDetailDto
                    {
                        Id = x.Id,
                        InventoryMasterItemId = x.InventoryMasterItemId,
                        Name = x.InventoryMasterItem.Name,
                        Group = (int)x.InventoryMasterItem.Group,
                        GroupName = x.InventoryMasterItem.Group.ToString(),
                        IsPresent = x.IsPresent
                    })
                    .ToList()
            };
        }
    }
}
