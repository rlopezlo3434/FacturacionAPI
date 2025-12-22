namespace FacturacionAPI.Models.Entities
{
    public class AnulacionDocumento
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public Venta venta { get; set; }
        public string Motivo { get; set; }
        public string CodigoUnico { get; set; }
        public string EnlacePdf { get; set; }
        public string EnlaceXml { get; set; }
        public string EnlaceCdr { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
