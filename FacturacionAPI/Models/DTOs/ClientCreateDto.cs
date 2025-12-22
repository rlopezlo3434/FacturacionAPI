using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class ClientCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DocumentIdentificationType DocumentIdentificationType { get; set; }
        public string DocumentIdentificationNumber { get; set; }
        public string? Email { get; set; }
        public GenderEnum Gender { get; set; }
        public bool AcceptsMarketing { get; set; }
        public List<string>? Numbers { get; set; } 
    }
}
