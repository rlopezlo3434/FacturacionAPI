namespace FacturacionAPI.Models.DTOs
{
    public class CreateVehicleIntakeDto
    {
        public int VehicleId { get; set; }
        public int ClientId { get; set; }

        public int Mode { get; set; } // 1 Taller, 2 domicilio
        public string? PickupAddress { get; set; }

        public int MileageKm { get; set; }
        public string? Observations { get; set; }
        public string? Services { get; set; }

        //public List<CreateVehicleIntakeInventoryItemDto> InventoryItems { get; set; } = new();
        public string InventoryItems { get; set; }
    }

    public class CreateVehicleIntakeInventoryItemDto
    {
        public int inventoryMasterItemId { get; set; }
        public bool isPresent { get; set; }
    }
}
