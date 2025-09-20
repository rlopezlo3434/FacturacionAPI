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

        [HttpGet("by-establishment")]
        public async Task<IActionResult> GetEmployeesByEstablishment(int establishmentId)
        {
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

            //var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            try
            {
                var message = await _employeeService.CreateEmployee(dto, currentUserId);
                return Ok(new { Success = true, Message = message });
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


        //[Authorize]
        //[HttpPost("create-client")]
        //public async Task <IActionResult> CreateClient([FromBody] ClientCreateDto dto)
        //{
        //    if(!ModelState.IsValid) return BadRequest(ModelState);

        //    var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

        //    try
        //    {
        //        var message = await _employeeService.CreateClient(dto, establishmentId);
        //        return Ok(new { Success = true, Message = message });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return StatusCode(StatusCodes.Status403Forbidden, new { Success = false, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }

        //}

    }
}
