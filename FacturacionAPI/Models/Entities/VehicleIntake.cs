using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleIntake
    {
        [Key]
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public IntakeModeEnum Mode { get; set; }

        [MaxLength(250)]
        public string? PickupAddress { get; set; } // solo si es recojo

        public int MileageKm { get; set; } // kilometraje al ingreso

        [MaxLength(500)]
        public string? Observations { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<VehicleIntakeInventoryItem> InventoryItems { get; set; } = new();

        public ICollection<VehicleIntakeImage> Images { get; set; } = new List<VehicleIntakeImage>();

        public ICollection<VehicleIntakeDiagram> ImagesDiagram { get; set; } = new List<VehicleIntakeDiagram>();

    }
}
