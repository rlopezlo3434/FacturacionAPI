using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class WorkOrderEmployee
    {
        [Key]
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }

}
