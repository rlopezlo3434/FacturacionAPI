using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class ChildrenClient : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime? FechaCumpleanios { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public bool IsActive { get; set; } = true;

        public string? Genero { get; set; }   
    }
}
