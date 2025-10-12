using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Item : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int? ProductDefinitionId { get; set; }
        public ProductDefinition ProductDefinition { get; set; }

        [Required]
        // Relaciones
        public int? EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }
        public decimal? Value { get; set; }

        public bool IsActive { get; set; } = true;

        public Stock Stock { get; set; }
    }
}
