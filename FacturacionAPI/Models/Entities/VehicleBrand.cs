using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleBrand
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public List<VehicleModel> Models { get; set; } = new();
    }
}
