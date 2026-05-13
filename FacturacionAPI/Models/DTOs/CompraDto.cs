namespace FacturacionAPI.Models.DTOs
{
    public class CompraDetalleCreateDto
    {
        public int ProductId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
    }

    public class CompraCreateDto
    {
        public int ProveedorId { get; set; }
        public string? Serie { get; set; }
        public string? Correlativo { get; set; }
        public DateTime FechaDocumento { get; set; }
        public string Moneda { get; set; } = "SOLES";
        public List<CompraDetalleCreateDto> Detalles { get; set; } = new();
    }

    public class CompraDetalleResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductNombre { get; set; } = "";
        public string ProductCodigo { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
    }

    public class CompraResponseDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorRazonSocial { get; set; } = "";
        public string ProveedorRuc { get; set; } = "";
        public string? Serie { get; set; }
        public string? Correlativo { get; set; }
        public DateTime FechaDocumento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = "";
        public List<CompraDetalleResponseDto> Detalles { get; set; } = new();
    }
}
