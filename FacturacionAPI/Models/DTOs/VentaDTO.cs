namespace FacturacionAPI.Models.DTOs
{
    public class VentaDetalleResponseDto
    {
        public int Id { get; set; }
        public string? TipoComprobante { get; set; }
        public string? Serie { get; set; }
        public int Numero { get; set; }
        public string? Correlativo { get; set; }
        public string? Direccion { get; set; }
        public string? ClienteDocumento { get; set; }
        public string? ClienteNombre { get; set; }

        public DateTime FechaEmision { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }

        public string? Observaciones { get; set; }

        public List<VentaDetalleItemDto?> Detalles { get; set; }

        public string? Pdf { get; set; }
        public string? Xml { get; set; }
        public string? Cdr { get; set; }
    }

    public class VentaDetalleItemDto
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
    }
}
