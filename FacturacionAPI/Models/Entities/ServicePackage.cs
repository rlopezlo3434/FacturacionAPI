using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ServicePackage
    {
        [Key]
        public int Id { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ServicePackageItem> Items { get; set; } = new();
    }
}
