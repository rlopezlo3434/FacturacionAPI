namespace FacturacionAPI.Models.DTOs
{
    public class WorkOrderListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int Correlativo { get; set; }
        public int VehicleIntakeId { get; set; }
        public int Mode { get; set; } // 1=taller, 2=recojo
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public VehicleMiniDto Vehicle { get; set; } = null!;
        public ClientMiniDto Client { get; set; } = null!;
    }

    public class VehicleMiniDto
    {
        public int Id { get; set; }
        public string Plate { get; set; } = null!;
        public CatalogMiniDto Brand { get; set; } = null!;
        public CatalogMiniDto Model { get; set; } = null!;
    }

    public class ClientMiniDto
    {
        public int Id { get; set; }
        public string Names { get; set; } = null!;
    }

    public class CatalogMiniDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class WorkOrderDetailDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int VehicleIntakeId { get; set; }
        public int Mode { get; set; }

        public string? Notes { get; set; }
        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public VehicleMiniDto Vehicle { get; set; } = null!;
        public ClientMiniDto Client { get; set; } = null!;

        public List<WorkOrderItemDto> Items { get; set; } = new();

        public List<EmployeeMiniDto> Employees { get; set; } = new();
        public List<ProveedorMiniDto> Proveedores { get; set; } = new();
    }

    public class WorkOrderItemDto
    {
        public int Id { get; set; }
        public int ItemType { get; set; } // 1=Producto, 2=Servicio
        public int BudgetItemId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public bool IsCompleted { get; set; }
        public string? Observations { get; set; }
    }

    public class WorkOrderUpdateItemsDto
    {
        public int WorkOrderId { get; set; }
        public List<int> EmpleadosIds { get; set; }
        public List<int> ProveedoresIds { get; set; }
        public List<WorkOrderItemUpdateDto> Items { get; set; } = new();
    }

    public class WorkOrderItemUpdateDto
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public string? Observations { get; set; }
    }

    public class EmployeeMiniDto
    {
        public int Id { get; set; }
        public string Names { get; set; }
    }

    public class WorkOrderReporteDto
    {
        public string Code { get; set; } = null!;
        public int Correlativo { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Mode { get; set; }
        public bool IsCompleted { get; set; }
        public string ClienteNombre { get; set; } = null!;
        public string Placa { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string Moneda { get; set; } = "PEN";
        public decimal Total { get; set; }
        public List<WorkOrderReporteItemDto> Items { get; set; } = new();
    }

    public class WorkOrderReporteItemDto
    {
        public string Tipo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
        public bool IsCompleted { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ProveedorMiniDto
    {
        public int Id { get; set; }
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
    }
}
