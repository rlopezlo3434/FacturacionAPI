using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class ChildrenDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? FechaCumpleanios { get; set; }
        public int ClientId { get; set; }
        public bool IsActive { get; set; } = true;
        public string Genero { get; set; }
    }
}
