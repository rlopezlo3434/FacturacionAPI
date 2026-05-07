using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VentaCuota
    {
        [Key]
        public int Id { get; set; }
        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public int NumeroCuota { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }
    }
}
