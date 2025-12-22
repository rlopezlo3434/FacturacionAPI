using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class StockMovementDto
    {
        public int ItemId { get; set; }

        public MovementType MovementType { get; set; } // Entrada o Salida

        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
