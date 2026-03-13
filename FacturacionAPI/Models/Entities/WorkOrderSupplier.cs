using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class WorkOrderSupplier
    {
        [Key]
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

    }
}
