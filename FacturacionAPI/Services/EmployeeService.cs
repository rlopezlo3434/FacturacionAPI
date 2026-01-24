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
                    Names = e.Names,
                    DocumentNumber = e.DocumentIdentificationNumber,
                    Email = e.Email,
                    Username = e.Username,
                    RoleName = e.Role.Name,
                    RoleCode = e.Role.Code,
                    Gender = e.Gender.ToString() == "M" ? "Hombre" : "Mujer",
                    TypeGender = e.Gender.ToString(),
                    DocumentIdentificationType = e.DocumentIdentificationType.ToString(),
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> UpdateEmployeeStateAsync(int id, bool estatus)
        {
            var employee = await _context.Employee.FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return (false, "Empleado no encontrado");

            employee.IsActive = estatus;

            _context.Employee.Update(employee);

            await _context.SaveChangesAsync();

            return (true, "Empleado actualizado correctamente");
        }

        public async Task<(bool Success, string Message)> UpdateEmployeeAsync(int id, UpdateEmployeeRequest emp)
        {
            var employee = await _context.Employee.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                return (false, "Empleado no encontrado");

            if (!string.IsNullOrEmpty(emp.Names))
                employee.Names = emp.Names;

            if (!string.IsNullOrEmpty(emp.DocumentIdentificationNumber))
                employee.DocumentIdentificationNumber = emp.DocumentIdentificationNumber;

            if (!string.IsNullOrEmpty(emp.DocumentIdentificationType))
                employee.DocumentIdentificationType = Enum.Parse<DocumentIdentificationType>(emp.DocumentIdentificationType);

            if (!string.IsNullOrEmpty(emp.Email))
                employee.Email = emp.Email;

            if (!string.IsNullOrEmpty(emp.Password))
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(emp.Password);

            if (!string.IsNullOrEmpty(emp.Gender))
                employee.Gender = Enum.Parse<GenderEnum>(emp.Gender);

            if (!string.IsNullOrEmpty(emp.RoleCode))
            {
                var newRole = await _context.Role.FirstOrDefaultAsync(r => r.Code == emp.RoleCode);
                if (newRole == null)
                    return (false, "Rol no encontrado");
                employee.RoleId = newRole.Id;
            }

            _context.Employee.Update(employee);
            await _context.SaveChangesAsync();

            return (true, "Empleado actualizado correctamente");
        }

        public async Task<(bool Success, string Message)> CreateEmployee(EmployeeCreateDto dto, int currentUserId, int establishmentId)
        {
            // 1. Buscar el usuario actual y su rol
            var currentUser = await _context.Employee
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == currentUserId);


            var existing = await _context.Employee.FirstOrDefaultAsync(e =>
                                                    e.Username == dto.Username ||
                                                    e.DocumentIdentificationNumber == dto.DocumentIdentificationNumber ||
                                                    e.Email == dto.Email);

            if (existing != null)
            {
                if (existing.Username == dto.Username)
                    return (false, "El nombre de usuario ya está en uso.");
                if (existing.DocumentIdentificationNumber == dto.DocumentIdentificationNumber)
                    return (false, "El número de documento ya está registrado.");
                if (existing.Email == dto.Email)
                    return (false, "El correo electrónico ya está en uso.");
            }

            // 2. Verificar rol que quiere asignar
            var newRole = await _context.Role.FirstOrDefaultAsync(r => r.Code == dto.RoleCode);

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
                Names = dto.Names,
                DocumentIdentificationType = dto.DocumentIdentificationType,
                DocumentIdentificationNumber = dto.DocumentIdentificationNumber,
                Email = dto.Email,
                EstablishmentId = establishmentId,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = newRole.Id,
                IsActive = true,
                Gender = Enum.Parse<GenderEnum>(dto.Gender) 
            };

            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();

            return (true,$"Empleado {employee.Names} creado con rol {newRole.Name}.");
        }

        //public async Task<string> CreateClient(ClientCreateDto dto, int establishmentId)
        //{

        //}

    }
}
