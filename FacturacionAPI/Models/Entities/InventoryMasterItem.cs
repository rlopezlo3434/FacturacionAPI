using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class InventoryMasterItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = null!;

        public InventoryGroupEnum Group { get; set; } = InventoryGroupEnum.InventarioVehiculo;

        public bool IsActive { get; set; } = true;
    }
}
