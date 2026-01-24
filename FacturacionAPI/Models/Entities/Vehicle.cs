using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(15)]
        public string Plate { get; set; } = null!;

        public int BrandId { get; set; }
        public VehicleBrand Brand { get; set; } = null!;

        public int ModelId { get; set; }
        public VehicleModel Model { get; set; } = null!;

        public int Year { get; set; }
        public string? Color { get; set; }

        public int? CurrentMileageKm { get; set; }
        public bool IsActive { get; set; } = true;

        public List<VehicleOwner> Owners { get; set; } = new();
    }

}
