using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Items : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ItemEnum Item { get; set; } // "PRODUCT", "SERVICE"
        
        [Required]
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Relaciones
        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }

        public Stock Stock { get; set; }
    }
}
