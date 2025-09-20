using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Role : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
