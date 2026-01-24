using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class UpdateEmployeeRequest
    {
        public string? Names { get; set; }

        public string? DocumentIdentificationType { get; set; }

        public string? DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; }  // "M" o "F"

        public string? Password { get; set; }   // luego se hashea

        public string? RoleCode { get; set; }        

    }
}
