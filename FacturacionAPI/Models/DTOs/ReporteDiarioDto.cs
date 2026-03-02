namespace FacturacionAPI.Models.DTOs
{
    public class ReporteDiarioDto
    {
        public int Id { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string ClienteDocumento { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string? Observaciones { get; set; }
        public string? MetodoPago { get; set; }
        public string EstadoSunat { get; set; }
        public List<string> EmpleadoLista { get; set; }
        public string Empleados { get; set; }

        public List<ReporteDetalleDto> Detalles { get; set; } = new List<ReporteDetalleDto>();
    }

    public class ReporteDetalleDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }

        public string Empleado { get; set; }
    }

}
