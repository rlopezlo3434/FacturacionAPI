namespace FacturacionAPI.Models.DTOs
{
    public class NubefactComprobante
    {
        public string operacion { get; set; }
        public int tipo_de_comprobante { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public int sunat_transaction { get; set; }
        public int cliente_tipo_de_documento { get; set; }
        public string cliente_numero_de_documento { get; set; }
        public string cliente_denominacion { get; set; }
        public string cliente_direccion { get; set; }
        public string fecha_de_emision { get; set; }
        public int moneda { get; set; }
        public decimal porcentaje_de_igv { get; set; }
        public decimal total_gravada { get; set; }
        public decimal total_igv { get; set; }
        public decimal total { get; set; }
        public bool enviar_automaticamente_a_la_sunat { get; set; }
        public bool enviar_automaticamente_al_cliente { get; set; }
        public string observaciones { get; set; }
        public List<NubefactItem> items { get; set; }
    }

    public class NubefactItem
    {
        public string unidad_de_medida { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal cantidad { get; set; }
        public decimal valor_unitario { get; set; }
        public decimal precio_unitario { get; set; }
        public int tipo_de_igv { get; set; }
        public decimal igv { get; set; }
        public decimal total { get; set; }
    }
}
