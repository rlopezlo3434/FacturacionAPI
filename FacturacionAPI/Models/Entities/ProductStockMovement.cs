using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionAPI.Models.Entities
{
    // Kardex de salidas/entradas para el catálogo de Productos (facturación/ventas),
    // independiente del kardex legado basado en Item/ProductDefinition.
    public class ProductStockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public MovementType MovementType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        // Referencia al documento que originó el movimiento (boleta/factura/interno)
        public string DocumentoTipo { get; set; } = string.Empty; // FACTURA / BOLETA / INTERNO
        public string DocumentoSerie { get; set; } = string.Empty;
        public int DocumentoNumero { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
