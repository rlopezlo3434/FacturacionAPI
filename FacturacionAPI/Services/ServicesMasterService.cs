using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class ServicesMasterService
    {
        private readonly SistemaVentasDbContext _context;

        public ServicesMasterService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicesMasterListDto>> GetAllAsync()
        {
            return await _context.ServicesMasters
                .OrderByDescending(x => x.Id)
                .Select(x => new ServicesMasterListDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateAsync(ServicesMasterCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "El nombre del servicio es obligatorio.");

            if (dto.Price < 0)
                return (false, "El precio no puede ser negativo.");

            var code = await GenerateCodeAsync();

            var service = new ServicesMaster
            {
                Code = code,
                Name = dto.Name.Trim(),
                Price = dto.Price,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.ServicesMasters.Add(service);
            await _context.SaveChangesAsync();

            return (true, "Servicio creado correctamente.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, ServicesMasterUpdateDto dto)
        {
            var service = await _context.ServicesMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (service == null)
                return (false, "Servicio no encontrado.");

            service.Name = dto.Name.Trim();
            service.Price = dto.Price;
            service.IsActive = dto.IsActive;
            service.UpdatedAt = DateTime.Now;

            _context.ServicesMasters.Update(service);
            await _context.SaveChangesAsync();

            return (true, "Servicio actualizado correctamente.");
        }

        private async Task<string> GenerateCodeAsync()
        {
            var lastCode = await _context.ServicesMasters
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastCode))
                return "SV0001";

            var numberPart = lastCode.Replace("SV", "");

            if (!int.TryParse(numberPart, out int lastNumber))
                return "SV0001";

            var newNumber = lastNumber + 1;
            return $"SV{newNumber:D4}";
        }
    }
}
