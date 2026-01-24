using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class WorkOrderItem
    {
        [Key]
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        public BudgetItemType ItemType { get; set; } // Product o Service

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceMasterId { get; set; }
        public ServicesMaster? ServiceMaster { get; set; }

        public int Quantity { get; set; } = 1;

        public string? Observations { get; set; } // opcional para mecánico

        public bool IsCompleted { get; set; } = false; // estado mecánico
    }
}
