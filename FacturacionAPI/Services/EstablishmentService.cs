using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class EstablishmentService
    {
        private readonly SistemaVentasDbContext _context;

        public EstablishmentService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EstablishmentDto>> GetEstablishment()
        {
            return await _context.Establishment
                .Where(x => x.IsActive == true)
                .Select(e => new  EstablishmentDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }
    }
}
