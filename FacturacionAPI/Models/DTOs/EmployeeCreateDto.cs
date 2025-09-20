using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class EmployeeCreateDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string DocumentIdentificationType { get; set; }

        [Required]
        public string DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        [Required]
        public int EstablishmentId { get; set; }

        [Required]
        public string Gender { get; set; }  // "M" o "F"

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }   // luego se hashea

        [Required]
        public int RoleId { get; set; }        // rol que se asignará al nuevo empleado
    }
}
