using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Establishment : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? FullAddress { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CompanyId { get; set; }

        // Propiedad de navegación
        public Companie Company { get; set; }
    }
}
