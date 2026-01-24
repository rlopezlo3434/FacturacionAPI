using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ServicesMaster
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = null!; // SV0001, SV0002...

        public string Name { get; set; } = null!; // "Cambio de aceite"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
