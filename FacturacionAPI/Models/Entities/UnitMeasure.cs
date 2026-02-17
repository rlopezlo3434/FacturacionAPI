using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class UnitMeasure
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string Code { get; set; } = null!; // UND, L, GL, M, JGO

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!; // Unidad, Litro, Galón

        public bool IsActive { get; set; } = true;
    }
}
