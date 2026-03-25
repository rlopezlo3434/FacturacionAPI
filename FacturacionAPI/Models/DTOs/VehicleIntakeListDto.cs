using FacturacionAPI.Migrations;
using FacturacionAPI.Models.Entities;

namespace FacturacionAPI.Models.DTOs
{
    public class VehicleIntakeListDto
    {
        public int Id { get; set; }
        public int Mode { get; set; }
        public string? PickupAddress { get; set; }
        public int MileageKm { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public VehicleIntakeVehicleDto Vehicle { get; set; } = null!;
        public VehicleIntakeClientDto Client { get; set; } = null!;
    }

    public class VehicleIntakeVehicleDto
    {
        public int Id { get; set; }
        public string Plate { get; set; } = null!;
        public CatalogItemDto Brand { get; set; } = null!;
        public CatalogItemDto Model { get; set; } = null!;
    }

    public class VehicleIntakeClientDto
    {
        public int Id { get; set; }
        public string Names { get; set; } = null!;
    }

    public class VehicleIntakeDetailDto
    {
        public int Id { get; set; }
        public int Mode { get; set; }
        public string? PickupAddress { get; set; }
        public int MileageKm { get; set; }
        public string? Observations { get; set; }
        public string? Services { get; set; }
        public DateTime CreatedAt { get; set; }

        public VehicleMiniDto2 Vehicle { get; set; } = null!;
        public ClientMiniDto2 Client { get; set; } = null!;

        public List<VehicleIntakeInventoryDetailDto> InventoryItems { get; set; } = new();

        public List<VehicleIntakeImageDto> Images { get; set; } = new();
        public List<VehicleIntakeDiagram> ImagesDiagram { get; set; } = new();
    }

    public class VehicleIntakeDetailDto2
    {
        public int Id { get; set; }
        public int Mode { get; set; }
        public string? PickupAddress { get; set; }
        public int MileageKm { get; set; }
        public string? Observations { get; set; }
        public string? Services { get; set; }
        public DateTime CreatedAt { get; set; }

        public VehicleDto Vehicle { get; set; } = null!;
        public Client Client { get; set; } = null!;

        //public List<VehicleIntakeInventoryDetailDto> InventoryItems { get; set; } = new();

        //public List<VehicleIntakeImageDto> Images { get; set; } = new();
        //public List<VehicleIntakeDiagram> ImagesDiagram { get; set; } = new();
    }

    public class VehicleIntakeInventoryDetailDto
    {
        public int Id { get; set; }
        public int InventoryMasterItemId { get; set; }
        public string Name { get; set; } = null!;
        public int Group { get; set; }
        public string GroupName { get; set; } = null!;
        public bool IsPresent { get; set; }
    }

    public class VehicleMiniDto2
    {
        public int Id { get; set; }
        public string Plate { get; set; } = null!;
        public CatalogMiniDto2 Brand { get; set; } = null!;
        public CatalogMiniDto2 Model { get; set; } = null!;
    }

    public class ClientMiniDto2
    {
        public int Id { get; set; }
        public string Names { get; set; } = null!;
    }

    public class CatalogMiniDto2
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class VehicleIntakeImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

}
