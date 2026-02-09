namespace FacturacionAPI.Models.DTOs
{
    public class OwnerKpisDto
    {
        public decimal VentasMtd { get; set; }
        public decimal VentasVariacion { get; set; }

        public int ServiciosMtd { get; set; }
        public decimal ServiciosVariacion { get; set; }

        public decimal TicketPromedio { get; set; }
        public decimal TicketVariacion { get; set; }

        public decimal DesviacionSoles { get; set; }
    }
}
