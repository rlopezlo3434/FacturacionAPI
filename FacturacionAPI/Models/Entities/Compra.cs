using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionAPI.Models.Entities
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        [MaxLength(20)]
        public string? Serie { get; set; }

        [MaxLength(20)]
        public string? Correlativo { get; set; }

        public DateTime FechaDocumento { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Igv { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // SOLES | DOLARES
        [Required]
        [MaxLength(10)]
        public string Moneda { get; set; } = "SOLES";

        public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
    }
}
