using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionAPI.Models.Entities
{
    public class CompraDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompraId { get; set; }
        public Compra Compra { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }
    }
}
