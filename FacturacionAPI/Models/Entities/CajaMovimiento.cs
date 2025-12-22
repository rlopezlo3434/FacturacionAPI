namespace FacturacionAPI.Models.Entities
{
    public class CajaMovimiento
    {
        public int Id { get; set; }

        public int CajaAperturaId { get; set; }
        public CajaApertura CajaApertura { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public decimal Monto { get; set; }

        // INGRESO o EGRESO
        public string Tipo { get; set; } = "INGRESO";

        // VentaId si el movimiento viene de una venta
        public int? VentaId { get; set; }
        public Venta Venta { get; set; }

        // Descripción libre cuando es movimiento manual
        public string Motivo { get; set; } = string.Empty;

    }
}
