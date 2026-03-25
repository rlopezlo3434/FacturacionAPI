namespace FacturacionAPI.Models.DTOs
{
    public class InvoiceSelectableItemDto
    {
        public int BudgetItemId { get; set; }

        public string BudgetCode { get; set; } = null!;

        public string IntakeCode { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int ItemType { get; set; } // 1 Producto | 2 Servicio

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }

        public decimal SubTotal { get; set; }

        public bool Selected { get; set; } = false;

        public bool Invoiced { get; set; } = false;

        public int? ServicePackageId { get; set; }
}
}
