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
}
