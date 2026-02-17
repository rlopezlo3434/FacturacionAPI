using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }

        public string? Names { get; set; }

        public CatalogItemDto DocumentIdentificationType { get; set; } = default!;

        public string? DocumentIdentificationNumber { get; set; }

        public string? Email { get; set; }

        public CatalogItemDto Gender { get; set; } = default!;

        public List<ClientContactDto> Numbers { get; set; }

        public List<ClientAddresses> Addresses { get; set; }

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

    public class ClientContactDto
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string? ContactName { get; set; }
        public int Type { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class ClientAddresses
    {
        public int Id { get; set; }
        public string? AddressName { get; set; }
        public string? Address {  get; set; }
        public bool IsPrimary { get; set; }
    }
}
