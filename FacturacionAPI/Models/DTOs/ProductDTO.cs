using FacturacionAPI.Models.Entities;

namespace FacturacionAPI.Models.DTOs
{
    public class ProductCreateUpdateDto
    {
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public string? SerialCode { get; set; }
        public int? UnitMeasureId { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }
        public decimal? CostDolar { get; set; }
        public decimal? PriceDolar { get; set; }

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
        public UnitMeasure? UnitMeasure { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }
        public decimal? CostDolar { get; set; }
        public decimal? PriceDolar { get; set; }

        public bool IsMultiBrand { get; set; }
        public CatalogItemDto? Brand { get; set; }
    }

    public class ProductKardexMovementDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string MovementType { get; set; } = null!; // Entrada / Salida
        public decimal Quantity { get; set; }
        public string DocumentoTipo { get; set; } = null!;
        public string DocumentoSerie { get; set; } = null!;
        public int DocumentoNumero { get; set; }
        public int? VentaId { get; set; }
        public string? ClienteNombre { get; set; }
        public int? CompraId { get; set; }
        public string? ProveedorNombre { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
