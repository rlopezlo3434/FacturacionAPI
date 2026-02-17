using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleBudgetController : ControllerBase
    {
        private readonly VehicleBudgetService _service;

        public VehicleBudgetController(VehicleBudgetService service)
        {
            _service = service;
        }

        // ✅ GET api/VehicleBudget/intake/5
        [HttpGet("intake/{intakeId}")]
        public async Task<IActionResult> GetByIntake(int intakeId)
        {
            var data = await _service.GetBudgetsByIntakeAsync(intakeId);
            return Ok(new { success = true, data });
        }

        // ✅ POST api/VehicleBudget
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleBudgetCreateDto dto)
        {
            var result = await _service.CreateBudgetAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("approve-items")]
        public async Task<IActionResult> ApproveItems([FromBody] BudgetApprovalRequestDto dto)
        {
            var result = await _service.ApproveBudgetItemsAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ PUT api/VehicleBudget/approve/10
        [HttpPut("approve/{budgetId}")]
        public async Task<IActionResult> Approve(int budgetId)
        {
            var result = await _service.ApproveBudgetAsync(budgetId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ GET api/VehicleBudget/{budgetId}
        [HttpGet("{budgetId}")]
        public async Task<IActionResult> GetDetail(int budgetId)
        {
            var data = await _service.GetBudgetDetailAsync(budgetId);
            if (data == null)
                return NotFound(new { success = false, message = "Presupuesto no encontrado." });

            return Ok(new { success = true, data });
        }
    }
}
