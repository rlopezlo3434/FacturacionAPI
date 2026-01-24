namespace FacturacionAPI.Models.DTOs
{
    public class WorkOrderListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
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
    }

    public class WorkOrderItemDto
    {
        public int Id { get; set; }
        public int ItemType { get; set; } // 1=Producto, 2=Servicio
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public bool IsCompleted { get; set; }
        public string? Observations { get; set; }
    }

    public class WorkOrderUpdateItemsDto
    {
        public int WorkOrderId { get; set; }
        public List<WorkOrderItemUpdateDto> Items { get; set; } = new();
    }

    public class WorkOrderItemUpdateDto
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public string? Observations { get; set; }
    }

}
