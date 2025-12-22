using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [Authorize]
        [HttpGet("by-establishment")]
        public async Task<IActionResult> GetEmployeesByEstablishment()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var employees = await _employeeService.GetEmployeesByEstablishment(establishmentId);
            if (employees == null || !employees.Any())
                return NotFound(new { Message = "No se encontraron empleados para este establecimiento" });

            return Ok(employees);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = int.Parse(User.FindFirst("id").Value);

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var result = await _employeeService.CreateEmployee(dto, currentUserId, establishmentId);

                if (!result.Success)
                    return BadRequest(new { Success = false, Message = result.Message });


                return Ok(new { Success = true, Message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("updateState/{id}")]
        public async Task<IActionResult> UpdateState(int id, [FromBody] UpdateStateRequest request)
        {
            if (request.IsActive == null)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var result = await _employeeService.UpdateEmployeeStateAsync(id, request.IsActive);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new { Success = true, Message = "Estado actualizado correctamente" });
        }

        [HttpPost("updateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {

            var result = await _employeeService.UpdateEmployeeAsync(id, request);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new { Success = true, Message = "Empleado actualizado correctamente" });

        }
        

    }
}
