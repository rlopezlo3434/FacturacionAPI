using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Companie : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DocumentIdentificationType DocumentIdentificationType { get; set; } // "RUC" o "DNI"

        [Required]
        public string? DocumentIdentificationNumber { get; set; }

        public string? CommercialName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
