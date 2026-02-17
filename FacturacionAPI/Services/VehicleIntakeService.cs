using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Numerics;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace FacturacionAPI.Services
{
    public class VehicleIntakeService
    {
        private readonly SistemaVentasDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VehicleIntakeService(SistemaVentasDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        public async Task<(bool Success, string Message)> CreateVehicleIntakeAsync(CreateVehicleIntakeDto dto, List<IFormFile>? images, List<IFormFile>? diagrams)
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

            var inventoryItems = JsonSerializer.Deserialize<List<CreateVehicleIntakeInventoryItemDto>>(dto.InventoryItems);

            var details = inventoryItems!.Select(x => new VehicleIntakeInventoryItem
            {
                VehicleIntakeId = intake.Id,
                InventoryMasterItemId = x.inventoryMasterItemId,
                IsPresent = x.isPresent
            }).ToList();

            _context.VehicleIntakeInventoryItems.AddRange(details);

            if (images != null && images.Any())
            {
                // 📂 wwwroot físico del API
                var wwwRootPath = _env.WebRootPath;

                // 📂 wwwroot/Intakes
                var basePath = Path.Combine(wwwRootPath, "Intakes");

                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 📂 wwwroot/Intakes/{intakeId}
                var intakeFolder = Path.Combine(basePath, intake.Id.ToString());

                if (!Directory.Exists(intakeFolder))
                {
                    Directory.CreateDirectory(intakeFolder);
                }

                foreach (var image in images)
                {
                    if (!image.ContentType.StartsWith("image/"))
                        continue;

                    using var imageStream = image.OpenReadStream();
                    using var img = await Image.LoadAsync(imageStream);

                    // 🔧 Resize si es grande
                    if (img.Width > 1280)
                    {
                        img.Mutate(x =>
                            x.Resize(new ResizeOptions
                            {
                                Size = new Size(1280, 0),
                                Mode = ResizeMode.Max
                            })
                        );
                    }

                    var fileName = $"{Guid.NewGuid()}.jpg";

                    // 📌 Ruta física final
                    var fullPath = Path.Combine(intakeFolder, fileName);

                    await img.SaveAsync(
                        fullPath,
                        new JpegEncoder
                        {
                            Quality = 75
                        }
                    );

                    // 🌐 URL pública (IIS la sirve sola)
                    _context.VehicleIntakeImages.Add(new VehicleIntakeImage
                    {
                        VehicleIntakeId = intake.Id,
                        ImageUrl = $"/Intakes/{intake.Id}/{fileName}",
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }

            if (diagrams != null && diagrams.Any())
            {
                var basePath = Path.Combine(
                    _env.WebRootPath,
                    "Intakes",
                    intake.Id.ToString(),
                    "diagrams"
                );

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                foreach (var diagram in diagrams)
                {
                    if (!diagram.ContentType.StartsWith("image/"))
                        continue;

                    var fileName = diagram.FileName; // diagram-1.png
                    var fullPath = Path.Combine(basePath, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await diagram.CopyToAsync(stream);

                    _context.VehicleIntakeDiagram.Add(new VehicleIntakeDiagram
                    {
                        VehicleIntakeId = intake.Id,
                        MarkedImageUrl =
                            $"/Intakes/{intake.Id}/diagrams/{fileName}",
                        CreatedAt = DateTime.Now
                    });
                }
            }

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
                .Include(x => x.Images)
                .Include(x => x.ImagesDiagram)
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
                    .ToList(),
                Images = intake.Images
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new VehicleIntakeImageDto
                    {
                        Id = x.Id,
                        ImageUrl = x.ImageUrl
                    })
                    .ToList(),
                ImagesDiagram = intake.ImagesDiagram
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new VehicleIntakeDiagram{
                        Id = x.Id,
                        VehicleIntakeId = x.VehicleIntakeId,
                        MarkedImageUrl = x.MarkedImageUrl
                    }).ToList()
                };
        }
    }
}
