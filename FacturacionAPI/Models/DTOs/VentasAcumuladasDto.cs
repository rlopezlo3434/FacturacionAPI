namespace FacturacionAPI.Models.DTOs
{
    public class VentasAcumuladasDto
    {
        public int Dia { get; set; }
        public decimal MesActual { get; set; }
        public decimal MesAnterior { get; set; }
    }
}
