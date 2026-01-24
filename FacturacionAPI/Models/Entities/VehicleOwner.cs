using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.Entities
{
    public class VehicleOwner
    {
        [Key]
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        public bool IsCurrentOwner { get; set; } = true;
    }

}
