namespace FacturacionAPI.Models.DTOs
{
    public class VehicleBudgetCreateDto
    {
        public int VehicleIntakeId { get; set; }
        public string? Notes { get; set; }
        public List<VehicleBudgetItemCreateDto> Items { get; set; } = new();
    }

    public class VehicleBudgetItemCreateDto
    {
        public int ItemType { get; set; } // 1 Product, 2 Service

        public int? ProductId { get; set; }
        public int? ServiceMasterId { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }
    }

    public class VehicleBudgetListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public bool IsApproved { get; set; }
        public bool IsOfficial { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VehicleBudgetDetailDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public bool IsApproved { get; set; }
        public bool IsOfficial { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<VehicleBudgetItemDetailDto> Items { get; set; } = new();
    }

    public class VehicleBudgetItemDetailDto
    {
        public int Id { get; set; }
        public int ItemType { get; set; } // 1 product, 2 service
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public CatalogItemDto? Product { get; set; }
        public CatalogItemDto? Service { get; set; }
    }
}
