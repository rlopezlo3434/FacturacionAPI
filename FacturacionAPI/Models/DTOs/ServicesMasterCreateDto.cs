namespace FacturacionAPI.Models.DTOs
{
    public class ServicesMasterCreateDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class ServicesMasterUpdateDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServicesMasterListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
