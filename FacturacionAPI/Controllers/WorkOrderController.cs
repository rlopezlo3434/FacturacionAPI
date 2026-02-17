using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController : ControllerBase
    {
        private readonly WorkOrderService _service;

        public WorkOrderController(WorkOrderService service)
        {
            _service = service;
        }

        // ✅ POST api/WorkOrder/create-from-intake/15
        [HttpPost("create-from-intake/{intakeId}")]
        public async Task<IActionResult> CreateFromIntake(int intakeId)
        {
            var result = await _service.CreateWorkOrderFromOfficialBudgetAsync(intakeId);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("from-intake/{intakeId}")]
        public async Task<IActionResult> GenerateWorkOrder(int intakeId)
        {
            var result = await _service.GenerateOrUpdateWorkOrderAsync(intakeId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ✅ GET api/WorkOrder
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetWorkOrdersAsync();
            return Ok(new { success = true, data = result });
        }

        // ✅ GET api/WorkOrder/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetWorkOrderDetailAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "Orden de trabajo no encontrada." });

            return Ok(new { success = true, data = result });
        }

        // ✅ PUT api/WorkOrder/update-items
        [HttpPut("update-items")]
        public async Task<IActionResult> UpdateItems([FromBody] WorkOrderUpdateItemsDto dto)
        {
            var result = await _service.UpdateWorkOrderItemsAsync(dto);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
