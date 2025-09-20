using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FacturacionAPI.Models.Entities
{
    public class Employee : BaseEntity
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

        // Relaciones
        public int EstablishmentId { get; set; }   
        public Establishment Establishment { get; set; }


        //Control de acceso
        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public int RoleId { get; set; }   
        public Role Role { get; set; }       

        public bool IsActive { get; set; } = true;
    }
}
