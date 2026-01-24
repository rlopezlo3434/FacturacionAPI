using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class ClientCreateDto
    {
        public string Names { get; set; }
        public DocumentIdentificationType DocumentIdentificationType { get; set; }
        public string DocumentIdentificationNumber { get; set; }
        public string? Email { get; set; }
        public GenderEnum Gender { get; set; }
        public bool AcceptsMarketing { get; set; }
        public List<ClientContactCreateDto> Numbers { get; set; } = new();
    }


    public class ClientContactCreateDto
    {
        public string? ContactName { get; set; }
        public int Type { get; set; } // 1..4 (ContactTypeEnum)
        public string Number { get; set; } = default!;
        public bool IsPrimary { get; set; }
    }
}
