using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class ServicePackageDto
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public List<ServicePackageItemDto> Items { get; set; } = new();
    }

    public class ServicePackageItemDto
    {
        public int Id { get; set; }

        public BudgetItemType ItemType { get; set; }

        public int? ProductId { get; set; }

        public int? ServiceMasterId { get; set; }

        public int Quantity { get; set; }
        public int ServicePackageId { get; set; }
    }

    public class ServicePackageCreateDto
    {
        public string? Description { get; set; }

        public List<ServicePackageItemCreateDto> Items { get; set; } = new();
    }

    public class ServicePackageItemCreateDto
    {
        public BudgetItemType ItemType { get; set; }

        public int? ProductId { get; set; }

        public int? ServiceMasterId { get; set; }

        public int Quantity { get; set; }
    }

    public class ServicePackageUpdateDto
    {
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public List<ServicePackageItemCreateDto> Items { get; set; } = new();
    }
}
