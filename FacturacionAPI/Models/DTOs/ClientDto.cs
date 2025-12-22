using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? DocumentIdentificationType { get; set; } // "DNI", "RUC"

        public string? DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; } //  "M", "F"

        public List<string> Numbers { get; set; } = new();

        public int EstablishmentId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AcceptsMarketing { get; set; } = false;
    }

    public class ChildrenClientDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? FechaCumpleanios { get; set; }
        public bool IsActive { get; set; }
        public ClientDto Client { get; set; }
    }
}
