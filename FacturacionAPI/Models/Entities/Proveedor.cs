using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(11)]
        public string Ruc { get; set; }

        [Required]
        [MaxLength(200)]
        public string RazonSocial { get; set; }

        [MaxLength(20)]
        public string Numero { get; set; }

        public bool Activo { get; set; } = true;
    }
}
