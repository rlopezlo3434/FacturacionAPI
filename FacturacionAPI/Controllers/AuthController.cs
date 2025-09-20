using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SistemaVentasDbContext _context;

        public AuthController(IConfiguration config, SistemaVentasDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = _context.Employee
                            .Include(e => e.Role)
                            .Include(e => e.Establishment)
                            .FirstOrDefault(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new AuthResponseDto
                {
                    Token = null,
                    Expiration = DateTime.MinValue,
                    Success = false,
                    User = null
                });

            // 2. Generar token
            var token = GenerateJwtToken(user);

            return Ok(token);
        }

        private AuthResponseDto GenerateJwtToken(Employee user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new[]
            {
            new Claim("id", user.Id.ToString()),
            new Claim("establishmentId", user.Establishment.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Success = true,
                Expiration = expiration,
                User = new EmployeeDto
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    LastName = user.LastName,
                    Establishment = new EstablishmentDto
                    {
                        Id = user.Establishment.Id,
                        Name = user.Establishment?.Name
                    },
                    DocumentNumber = user.DocumentIdentificationNumber,
                    Email = user.Email,
                    Username = user.Username,
                    RoleName = user.Role.Name,
                    Gender = user.Gender.ToString() == "M" ? "Hombre" : "Mujer",
                    IsActive = user.IsActive
                }
            };
        }
    }
}
