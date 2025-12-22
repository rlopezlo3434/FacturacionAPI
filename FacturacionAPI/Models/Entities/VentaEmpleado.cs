using FacturacionAPI.Models.DTOs;

namespace FacturacionAPI.Models.Entities
{
    public class VentaEmpleado
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public int EmpleadoId { get; set; }
        public Employee Empleado { get; set; }

        public int ProductDefinitionId { get; set; }
        public ProductDefinition productDefinition { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
