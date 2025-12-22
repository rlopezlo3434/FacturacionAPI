using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class VentaRequest
    {
        public List<VentaItem> items { get; set; }
        public decimal subtotalGeneral { get; set; }
        public decimal igvGeneral { get; set; }
        public decimal totalGeneral { get; set; }
        public string observaciones { get; set; }
        public int tipo_de_comprobante { get; set; }
        public string cliente_numero { get; set; }
        public string cliente_nombre { get; set; }
        public string serie { get; set; }
        public string cliente_tipo_documento { get; set; }
        public MetodoPago metodo_pago { get; set; }
        public DateTime fecha_emision { get; set; }

    }
    public class VentaItem
    {
        public int id { get; set; }
        public string code { get; set; }
        public string item { get; set; }
        public string description { get; set; }
        public decimal value { get; set; }
        public string createdAt { get; set; }
        public bool isActive { get; set; }
        public int cantidad { get; set; }
        public decimal subtotal { get; set; }
        public decimal igv { get; set; }
        public decimal total { get; set; }

        public List<Empleadoo> empleados { get; set; }

    }

    public class Empleadoo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }

    }
}
