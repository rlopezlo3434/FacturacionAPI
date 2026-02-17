using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleIntakeImage
    {
        [Key]
        public int Id { get; set; }

        public int VehicleIntakeId { get; set; }
        public VehicleIntake VehicleIntake { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
