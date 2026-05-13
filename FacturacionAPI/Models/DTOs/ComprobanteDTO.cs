using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class VentaRequest
    {
        public List<VentaItem> items { get; set; }
        public decimal? subtotalGeneral { get; set; }
        public decimal? igvGeneral { get; set; }
        public decimal? totalGeneral { get; set; }
        public string? observaciones { get; set; }
        public int? tipo_de_comprobante { get; set; }
        public string? cliente_numero { get; set; }
        public string? cliente_nombre { get; set; }
        public string? direccion { get; set; }
        public string? serie { get; set; }
        public string? cliente_tipo_documento { get; set; }
        public MetodoPago? metodo_pago { get; set; }
        public string? tipo_condicion_pago { get; set; } // CONTADO / CREDITO_15 / etc
        public List<VentaCuota> cuotas { get; set; }
        public DateTime? fecha_emision { get; set; }
        public bool detraccion { get; set; }
        public int? detraccion_tipo { get; set; }
        public string? cond_venta { get; set; }
        public decimal? detraccion_porcentaje { get; set; }
        public decimal? detraccion_total { get; set; }
        public string? vehiculo_placa { get; set; }

    }

    public class VentaCuota
    {
        public int Cuota { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }
    }
    public class VentaItem
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public decimal value { get; set; }
        public string? brand { get; set; }
        public string? model { get; set; }
        public string? placa { get; set; }
        public int? anio { get; set; }
        public bool isActive { get; set; }
        public decimal cantidad { get; set; }
        public decimal subtotal { get; set; }
        public decimal igv { get; set; }
        public decimal total { get; set; }


    }

    public class Empleadoo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }

    }
}
