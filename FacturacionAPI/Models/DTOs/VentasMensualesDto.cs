namespace FacturacionAPI.Models.DTOs
{
    public class VentasMensualesDto
    {
        public List<decimal> MontosPorDia { get; set; }
        public List<string> Dias { get; set; }
        public decimal TotalDia { get; set; }
        public decimal TotalMes { get; set; }
    }

    public class ServiciosMensualesDto
    {
        public List<decimal> ServiciosPorDia { get; set; }
        public List<string> Dias { get; set; }
        public decimal TotalDia { get; set; }
        public decimal TotalMes { get; set; }
    }

    public class TopServicioDto
    {
        public string Servicio { get; set; }
        public decimal Monto { get; set; }
    }

    public class ComparativoVentasDto
    {
        public List<string> Dias { get; set; }
        public List<decimal> MesActual { get; set; }
        public List<decimal> MesAnterior { get; set; }
    }

    public class ComparativoResponse
    {
        public decimal Actual { get; set; }
        public decimal Anterior { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ProductividadEmpleadoDto
    {
        public string Empleado { get; set; }
        public decimal Importe { get; set; }
        public decimal Cantidad { get; set; }
        public string Establecimiento { get; set; }
    }

    public class ContribucionEstilistaDto
    {
        public string Estilista { get; set; }
        public decimal Importe { get; set; }
    }


}
