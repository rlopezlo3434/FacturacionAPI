namespace FacturacionAPI.Models.DTOs
{
    public class CasillaDto
    {
        public int Id { get; set; }
        public bool Marcada { get; set; }
        public string? Label { get; set; }
        public DateTime? Fecha { get; set; }
    }
    public class TarjetaClienteResponse
    {
        public int ClienteId { get; set; }
        public int? ChildrenClientId { get; set; }
        public string? NombreHijo { get; set; }
        public List<CasillaDto> Casillas { get; set; }
        public int TotalVisitas { get; set; }
        public int DescuentoActual { get; set; }
    }

    public class ResetTarjetaRequest
    {
        public int ClienteId { get; set; }
    }

    public class ResetTarjetaHijoRequest
    {
        public int ChildrenClientId { get; set; }
    }

    public class RegistrarVisitaHijoRequest
    {
        public int ChildrenClientId { get; set; }
    }
}
