using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        public int VehicleBudgetItemId { get; set; }

        public VehicleBudgetItem VehicleBudgetItem { get; set; } = null!;

        public BudgetItemType ItemType { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceMasterId { get; set; }
        public ServicesMaster? ServiceMaster { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
