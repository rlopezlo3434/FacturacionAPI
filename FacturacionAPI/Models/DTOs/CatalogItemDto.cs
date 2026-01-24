namespace FacturacionAPI.Models.DTOs
{
    public class CatalogItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public class ModelDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string Name { get; set; } = null!;
        public bool isActive { get; set; }

    }

    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool isActive { get; set; }
    }
}
