using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleBudgetItem
    {
        [Key]
        public int Id { get; set; }

        public int VehicleBudgetId { get; set; }
        public VehicleBudget VehicleBudget { get; set; } = null!;

        public BudgetItemType ItemType { get; set; } // Product / Service

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceMasterId { get; set; }
        public ServicesMaster? ServiceMaster { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;
        public bool IsApproved { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool IsInWorkOrder { get; set; } = false;
    }
}
