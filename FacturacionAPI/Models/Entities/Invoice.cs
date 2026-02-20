using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public int VehicleIntakeId { get; set; }

        public decimal Total { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public List<InvoiceItem> Items { get; set; } = new();
    }
}
