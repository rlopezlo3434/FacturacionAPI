using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ClientNumbers
    {
        [Key]
        public int Id { get; set; }
        
        public int ClientId { get; set; }
        public Client Client { get; set; }

        [Required]
        public string Number { get; set; }
    }
}
