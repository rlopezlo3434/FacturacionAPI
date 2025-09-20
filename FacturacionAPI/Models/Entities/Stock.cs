using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Stock : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Items Item { get; set; }

        public int Quantity { get; set; } = 0; // stock actual
        public int MinStock { get; set; } = 0; // opcional, para alertas de reabastecimiento
    }
}
