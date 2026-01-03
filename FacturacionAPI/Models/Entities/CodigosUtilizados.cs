using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class CodigosUtilizados
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public string Code { get; set; }
    }
}
