namespace FacturacionAPI.Models.DTOs
{
    public class ApproveBudgetItemDto
    {
        public int ItemId { get; set; }
        public bool IsApproved { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class BudgetApprovalRequestDto
    {
        public int BudgetId { get; set; }
        public List<ApproveBudgetItemDto> Items { get; set; } = new();
    }
}
