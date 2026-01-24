namespace FacturacionAPI.Models.DTOs
{
    public class ProductCreateUpdateDto
    {
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public string? SerialCode { get; set; }

        public decimal Price { get; set; } // ✅ agregado

        public bool IsMultiBrand { get; set; }
        public int? BrandId { get; set; }
    }

    public class ProductListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public string? SerialCode { get; set; }

        public decimal Price { get; set; } // ✅ agregado

        public bool IsMultiBrand { get; set; }
        public CatalogItemDto? Brand { get; set; }
    }
}
