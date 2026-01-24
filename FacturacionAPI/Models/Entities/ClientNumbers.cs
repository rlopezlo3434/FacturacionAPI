using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ClientNumbers
    {
        [Key]
        public int Id { get; set; }
        
        public int ClientId { get; set; }
        public Client Client { get; set; }

        [MaxLength(50)]
        public string? ContactName { get; set; }

        // Tipo de contacto: Principal, Familiar, Trabajo, Otro
        public ContactTypeEnum Type { get; set; } = ContactTypeEnum.Otro;

        [Required]
        public string Number { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
