namespace FacturacionAPI.Models.DTOs
{
    public class VehicleListDto
    {
        public int Id { get; set; }

        public string Plate { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Vin { get; set; } = null!;

        public int Year { get; set; }
        public string? Color { get; set; }
        public int? CurrentMileageKm { get; set; }

        public bool IsActive { get; set; }

        public CatalogItemDto Brand { get; set; } = new();
        public CatalogItemDto Model { get; set; } = new();

        public CatalogItemDto? Owner { get; set; } // dueño actual (puede ser null)
    }

}
