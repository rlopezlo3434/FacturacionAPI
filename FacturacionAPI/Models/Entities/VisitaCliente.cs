namespace FacturacionAPI.Models.Entities
{
    public class VisitaCliente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public int CicloTarjeta { get; set; }

    }
}
