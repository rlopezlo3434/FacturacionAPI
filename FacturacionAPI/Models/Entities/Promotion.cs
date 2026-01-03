using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Promotion : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
