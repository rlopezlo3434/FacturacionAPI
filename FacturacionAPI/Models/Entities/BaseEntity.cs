using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public abstract class BaseEntity
    {
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
