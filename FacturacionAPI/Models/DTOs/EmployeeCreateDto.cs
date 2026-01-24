using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class EmployeeCreateDto
    {
        [Required]
        public string Names { get; set; }

        [Required]
        public DocumentIdentificationType DocumentIdentificationType { get; set; }

        [Required]
        public string DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        //[Required]
        //public int EstablishmentId { get; set; }

        [Required]
        public string Gender { get; set; }  // "M" o "F"

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }   // luego se hashea

        [Required]
        public string RoleCode { get; set; }        // rol que se asignará al nuevo empleado
    }
}
