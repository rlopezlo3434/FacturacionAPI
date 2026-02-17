using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ClientAddress
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        [MaxLength(50)]
        public string? AddressName { get; set; }

        [Required]
        public string Address { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
