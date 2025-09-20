using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class EmployeeService
    {
        private readonly SistemaVentasDbContext _context;

        public EmployeeService(SistemaVentasDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByEstablishment(int establishmentId)
        {
            return await _context.Employee
                .Include(e => e.Establishment)
                .Include(e => e.Role)
                .Where(e => e.EstablishmentId == establishmentId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.FirstName,
                    LastName = e.LastName,
                    DocumentNumber = e.DocumentIdentificationNumber,
                    Email = e.Email,
                    Username = e.Username,
                    RoleName = e.Role.Name,
                    Gender = e.Gender.ToString() == "M" ? "Hombre" : "Mujer",
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<string> CreateEmployee(EmployeeCreateDto dto, int currentUserId)
        {
            // 1. Buscar el usuario actual y su rol
            var currentUser = await _context.Employee
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == currentUserId);

            if (currentUser == null)
                throw new UnauthorizedAccessException("Usuario no encontrado.");

            // 2. Verificar rol que quiere asignar
            var newRole = await _context.Role.FindAsync(dto.RoleId);
            if (newRole == null)
                throw new Exception("Rol inválido.");

            // 3. Reglas de creación
            if (currentUser.Role.Code == "OWNER" && newRole.Code == "OWNER")
                throw new UnauthorizedAccessException("El dueño solo puede crear administradores y colaboradores.");

            if (currentUser.Role.Code == "ADMIN" && newRole.Code == "OWNER" && newRole.Code == "ADMIN")
                throw new UnauthorizedAccessException("El administrador solo puede crear colaboradores.");

            if (currentUser.Role.Code == "COLLAB")
                throw new UnauthorizedAccessException("Un colaborador no puede crear empleados.");

            // 4. Crear el nuevo empleado
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DocumentIdentificationType = Enum.Parse<DocumentIdentificationType>(dto.DocumentIdentificationType),
                DocumentIdentificationNumber = dto.DocumentIdentificationNumber,
                Email = dto.Email,
                EstablishmentId = dto.EstablishmentId,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                IsActive = true,
                Gender = Enum.Parse<GenderEnum>(dto.Gender) 
            };

            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();

            return $"Empleado {employee.FirstName} {employee.LastName} creado con rol {newRole.Name}.";
        }

        //public async Task<string> CreateClient(ClientCreateDto dto, int establishmentId)
        //{

        //}

    }
}
