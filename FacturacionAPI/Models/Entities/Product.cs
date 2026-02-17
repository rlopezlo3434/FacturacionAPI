using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; } = null!; // PR0001

        public string Name { get; set; } = null!;

        public int Quantity { get; set; } = 0;

        public string? SerialCode { get; set; } // Ejm P240

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public int? UnitMeasureId { get; set; }
        public UnitMeasure UnitMeasure { get; set; } = null!;

        public bool IsMultiBrand { get; set; } = false;

        public int? BrandId { get; set; }
        public VehicleBrand? Brand { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
