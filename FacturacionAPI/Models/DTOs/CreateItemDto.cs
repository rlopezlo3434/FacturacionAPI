using FacturacionAPI.Models.Enums;

namespace FacturacionAPI.Models.DTOs
{
    public class CreateItemDto
    {
        public ItemEnum Item { get; set; } // PRODUCT o SERVICE
        public string Description { get; set; }
        public int EstablishmentId { get; set; }

        // Opcional para productos
        public int InitialQuantity { get; set; } = 0;
        public int MinStock { get; set; } = 0;
    }
}
