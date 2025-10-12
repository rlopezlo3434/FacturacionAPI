using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ProductDefinition : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; } // código único global (ej: "SHAMPOO-001")

        [Required]
        public ItemEnum Item { get; set; } // PRODUCT o SERVICE

        [Required]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
