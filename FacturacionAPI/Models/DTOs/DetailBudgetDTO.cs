namespace FacturacionAPI.Models.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Plate { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public int Year { get; set; }
        public string? Color { get; set; }

        public int? CurrentMileageKm { get; set; }

        public BrandDto Brand { get; set; } = null!;
        public ModelDto Model { get; set; } = null!;
    }

    
}
