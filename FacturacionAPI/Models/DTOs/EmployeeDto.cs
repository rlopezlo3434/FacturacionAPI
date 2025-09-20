using FacturacionAPI.Migrations;

namespace FacturacionAPI.Models.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? DocumentNumber { get; set; }
        public EstablishmentDto? Establishment { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? RoleName { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
    }

    public class EstablishmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
