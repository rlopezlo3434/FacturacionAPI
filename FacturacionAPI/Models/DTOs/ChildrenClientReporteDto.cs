namespace FacturacionAPI.Models.DTOs
{
    public class ChildrenClientReporteDto
    {
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool IsActive { get; set; }
        public bool AcceptsMarketing { get; set; }

        public string PhoneNumber { get; set; }

        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public DateTime? FechaCumpleanios { get; set; }
    }
}
