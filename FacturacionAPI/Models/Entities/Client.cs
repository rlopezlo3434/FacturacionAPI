using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Client : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DocumentIdentificationType DocumentIdentificationType { get; set; } // "DNI", "RUC"

        [Required]
        public string? DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        [Required]
        public GenderEnum Gender { get; set; } //  "M", "F"

        public ICollection<ClientNumbers> Numbers { get; set; } = new List<ClientNumbers>();

        // Relaciones
        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AcceptsMarketing { get; set; } = false;
    }
}
