using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class DocumentoPendienteEnvio
    {
        [Key]
        public int Id { get; set; }

        // "NOTA_CREDITO" | "ANULACION"
        public string TipoDocumento { get; set; } = string.Empty;

        public int VentaOriginalId { get; set; }
        public Venta VentaOriginal { get; set; } = null!;

        // Solo para NC: serie-número del comprobante nuevo (ej: "F002-15")
        public string? ReemplazadoPor { get; set; }

        // Cuándo debe enviarse a Nubefact
        public DateTime FechaProgramada { get; set; }

        // "PENDIENTE" | "ENVIADO" | "ERROR"
        public string Estado { get; set; } = "PENDIENTE";

        public int Intentos { get; set; } = 0;

        public string? MensajeError { get; set; }

        // VentaId de la NC creada tras el envío exitoso
        public int? NcVentaId { get; set; }
        public Venta? NcVenta { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? EnviadoAt { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;
    }
}
