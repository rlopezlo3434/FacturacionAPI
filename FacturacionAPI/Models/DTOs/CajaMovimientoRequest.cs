using FacturacionAPI.Models.Entities;

namespace FacturacionAPI.Models.DTOs
{
    public class CajaMovimientoRequest
    {

        public int CajaAperturaId { get; set; }


        public decimal Monto { get; set; }

        public string Tipo { get; set; }


        public string Motivo { get; set; }
    }

    public class CerrarCajaRequest
    {
        public int CajaAperturaId { get; set; }
        public decimal EfectivoContado { get; set; }
        public string Observaciones { get; set; }
    }
}
