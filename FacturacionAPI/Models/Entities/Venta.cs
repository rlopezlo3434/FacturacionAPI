using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.Entities
{
    public class Venta
    {
        public int Id { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string ClienteDocumento { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public decimal TotalGravada { get; set; }
        public decimal TotalIgv { get; set; }
        public decimal Total { get; set; }
        
        // 🔹 DETRACCIÓN
        public bool Detraccion { get; set; } = false;
        public int? DetraccionTipo { get; set; }
        public decimal? DetraccionPorcentaje { get; set; }
        public decimal? DetraccionMonto { get; set; }
        
        public DateTime FechaEmision { get; set; }
        public string? Observaciones { get; set; }
        public string? CodigoHash { get; set; }
        public string? EnlacePdf { get; set; }
        public string? EnlaceXml { get; set; }
        public string? EnlaceCdr { get; set; }
        public bool IsAnnulled { get; set; } = false;
        public bool UsadoParaDescuento { get; set; } = false;

        public int? EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }

        public MetodoPago MetodoPago { get; set; }
        public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    }
}
