using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class WorkOrder
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; } = null!; // OT-00015-001

        public int VehicleIntakeId { get; set; }
        public VehicleIntake VehicleIntake { get; set; } = null!;

        //public int? BudgetId { get; set; } // presupuesto oficial
        //public VehicleBudget Budget { get; set; } = null!;

        public string? Notes { get; set; } // notas internas para mecánicos

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<WorkOrderItem> Items { get; set; } = new();

        // 🔹 RELACIONES NUEVAS
        public List<WorkOrderEmployee> WorkOrderEmployees { get; set; } = new();

        public List<WorkOrderSupplier> WorkOrderSuppliers { get; set; } = new();
    }
}
