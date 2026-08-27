using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class Establishment : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? FullAddress { get; set; }

        public DocumentIdentificationType DocumentIdentificationType { get; set; } // "RUC" o "DNI"

        public string? DocumentIdentificationNumber { get; set; }

        public string? urlNubefact { get; set; }
        public string? TokenNubefact { get; set; }
        public string? SerieBoleta { get; set; }
        public string? SerieFactura { get; set; }
        public string? SerieNotaCredito { get; set; }   // NC para Boletas (ej: BC02)
        public string? SerieNotaCredito2 { get; set; }  // NC para Facturas (ej: FC02)

        public bool IsActive { get; set; } = true;

        [Required]
        public int CompanyId { get; set; }

        // Propiedad de navegación
        public Companie Company { get; set; }
    }
}
