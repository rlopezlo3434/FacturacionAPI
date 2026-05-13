namespace FacturacionAPI.Models.DTOs
{
    public class ProveedorCreateDto
    {
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
    }

    public class ProveedorResponseDto
    {
        public int Id { get; set; }
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
