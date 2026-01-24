using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleBudget
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = null!; // B0001, B0002...

        // Relación con internamiento
        public int VehicleIntakeId { get; set; }
        public VehicleIntake VehicleIntake { get; set; } = null!;

        // Estado
        public bool IsApproved { get; set; } = false;
        public bool IsOfficial { get; set; } = false; // ✅ solo uno por intake

        // ✅ AQUÍ va
        public bool IsActive { get; set; } = true;

        // Totales
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }

        // Detalle
        public List<VehicleBudgetItem> Items { get; set; } = new();
    }
}
