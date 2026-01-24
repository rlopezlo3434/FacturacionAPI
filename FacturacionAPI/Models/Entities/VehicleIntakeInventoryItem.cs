using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleIntakeInventoryItem
    {
        [Key]
        public int Id { get; set; }

        public int VehicleIntakeId { get; set; }
        public VehicleIntake VehicleIntake { get; set; } = null!;

        public int InventoryMasterItemId { get; set; }
        public InventoryMasterItem InventoryMasterItem { get; set; } = null!;

        public bool IsPresent { get; set; } = false; // SI / NO
    }
}
