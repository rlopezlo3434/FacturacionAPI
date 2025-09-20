using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class StockMovement : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Items Item { get; set; }

        [Required]
        public MovementType MovementType { get; set; } // Entrada o Salida

        [Required]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
