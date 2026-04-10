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
        public string? Modelo { get; set; }
        public string? Marca { get; set; }
        public int? Anio { get; set; }
        public string? Placa { get; set; }
        public DateTime FechaEmision { get; set; }
        public bool Detraccion { get; set; } = false;
        public int? DetraccionTipo { get; set; }
        public decimal? DetraccionPorcentaje { get; set; }
        public decimal? DetraccionMonto { get; set; }
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
