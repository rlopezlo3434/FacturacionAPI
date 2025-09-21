using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI.Models.DTOs
{
    public class ItemsDto
    {
        public int Id { get; set; }

        public string? Item { get; set; }

        public string? Description { get; set; }
        public string? CreatedAt { get; set; }
        public bool IsActive { get; set; }

    }
}
