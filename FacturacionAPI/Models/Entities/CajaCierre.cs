namespace FacturacionAPI.Models.Entities
{
    public class CajaCierre
    {
        public int Id { get; set; }

        public int CajaAperturaId { get; set; }
        public CajaApertura CajaApertura { get; set; }

        public DateTime FechaCierre { get; set; } = DateTime.Now;

        public decimal EfectivoCalculado { get; set; }
        public decimal EfectivoContado { get; set; }

        public decimal Diferencia => EfectivoContado - EfectivoCalculado;

        public string Observaciones { get; set; } = string.Empty;

    }
}
