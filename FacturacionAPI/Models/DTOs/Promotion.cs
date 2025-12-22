using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class PromotionCreateDto
    {
        public string Name { get; set; }
        public PromotionTypeEnum Type { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PromotionUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public PromotionTypeEnum? Type { get; set; }
        public decimal? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PromotionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
