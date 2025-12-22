using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class PromotionService
    {
        private readonly SistemaVentasDbContext _context;

        public PromotionService(SistemaVentasDbContext context)
        {
            _context = context;
        }
        public async Task<(bool Success, string Message)> CreatePromotionAsync(PromotionCreateDto dto, int establishmentId)
        {
            var promo = new Promotion
            {
                Name = dto.Name,
                Type = dto.Type,
                Value = dto.Value,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                EstablishmentId = establishmentId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Promotion.Add(promo);
            await _context.SaveChangesAsync();
            return (true, "Promoción creada correctamente.");
        }

        public async Task<IEnumerable<PromotionDto>> GetPromotionsByEstablishmentAsync(int establishmentId)
        {
            return await _context.Promotion
                .Where(p => p.EstablishmentId == establishmentId)
                .Select(p => new PromotionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = p.Type.ToString(),
                    Value = p.Value,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> UpdatePromotionAsync(PromotionUpdateDto dto)
        {
            var promo = await _context.Promotion.FindAsync(dto.Id);
            if (promo == null)
                return (false, "No se encontró la promoción.");

            if (!string.IsNullOrEmpty(dto.Name)) promo.Name = dto.Name;
            if (dto.Type.HasValue) promo.Type = dto.Type.Value;
            if (dto.Value.HasValue) promo.Value = dto.Value.Value;
            if (dto.StartDate.HasValue) promo.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) promo.EndDate = dto.EndDate.Value;
            if (dto.IsActive.HasValue) promo.IsActive = dto.IsActive.Value;

            promo.UpdatedAt = DateTime.Now;

            _context.Promotion.Update(promo);
            await _context.SaveChangesAsync();

            return (true, "Promoción actualizada correctamente.");
        }
    }
}
