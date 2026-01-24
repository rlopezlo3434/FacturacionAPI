using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleModel
    {
        [Key]
        public int Id { get; set; }

        public int BrandId { get; set; }
        public VehicleBrand Brand { get; set; } = null!;

        [Required, MaxLength(80)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}
