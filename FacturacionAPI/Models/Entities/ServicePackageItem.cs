using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ServicePackageItem
    {
        [Key]
        public int Id { get; set; }

        public int ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = null!;

        public BudgetItemType ItemType { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceMasterId { get; set; }
        public ServicesMaster? ServiceMaster { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
