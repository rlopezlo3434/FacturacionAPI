namespace FacturacionAPI.Models.Entities
{
    public class CajaApertura
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }

        public decimal MontoApertura { get; set; }

        public DateTime FechaApertura { get; set; } = DateTime.Now;

        public bool Cerrada { get; set; } = false;

        // Relación con movimientos
        public ICollection<CajaMovimiento> Movimientos { get; set; } = new List<CajaMovimiento>();

        // Relación 1-1 con cierre
        public CajaCierre Cierre { get; set; }
    }
}
