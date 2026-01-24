namespace FacturacionAPI.Models.DTOs
{
    public class VehicleCreateDto
    {
        public int EstablishmentId { get; set; }
        public string Plate { get; set; } = null!;
        public int BrandId { get; set; }
        public int ModelId { get; set; }

        public int Year { get; set; }
        public string? Color { get; set; }
        public int? CurrentMileageKm { get; set; }

        public int OwnerClientId { get; set; }
    }

    public class VehicleUpdateDto
    {
        public int EstablishmentId { get; set; }
        public string Plate { get; set; } = null!;
        public int BrandId { get; set; }
        public int ModelId { get; set; }

        public int Year { get; set; }
        public string? Color { get; set; }
        public int? CurrentMileageKm { get; set; }

        public int OwnerClientId { get; set; } 
        public bool IsActive { get; set; } = true;
    }

    public class CreateBrandDto
    {
        public string Name { get; set; } = null!;
    }

    public class CreateModelDto
    {
        public int BrandId { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }
}
