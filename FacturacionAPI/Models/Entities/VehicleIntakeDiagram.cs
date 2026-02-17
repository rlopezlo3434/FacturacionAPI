using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleIntakeDiagram
    {
        [Key]
        public int Id { get; set; }

        public int VehicleIntakeId { get; set; }
        public VehicleIntake VehicleIntake { get; set; } = null!;

        public string MarkedImageUrl { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
