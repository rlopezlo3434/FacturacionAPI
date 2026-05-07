using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel;

namespace FacturacionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ClientService _clientService;

        public ClientController(ClientService clientService)
        {
            _clientService = clientService;
        }

        [Authorize]
        [HttpGet("by-establishment")]
        public async Task<IActionResult> GetClientByEstablishment()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var employees = await _clientService.GetClientByEstablishment(establishmentId);
            if (employees == null || !employees.Any())
                return NotFound(new { Message = "No se encontraron empleados para este establecimiento" });

            return Ok(employees);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            // obtener el establecimiento desde el token JWT
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var result = await _clientService.CreateClientAsync(dto, establishmentId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpPost("clientNumber/{id}")]
        public async Task<IActionResult> CreateClientNumber(int id, [FromBody] ClientNumbers dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.CreateClientNumber(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpPost("clientAddress/{id}")]
        public async Task<IActionResult> CreateClientAddress(int id, [FromBody] ClientAddress dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.CreateClientAddress(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("{clientId}/numbers")]
        public async Task<IActionResult> SaveNumbers(int clientId, [FromBody] List<ClientContactCreateDto> numbers)
        {
            try
            {
                await _clientService.SaveClientNumbersAsync(clientId, numbers);

                return Ok(new
                {
                    success = true,
                    message = "Números del cliente guardados correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("children")]
        public async Task<IActionResult> CreateChildrenClient([FromBody] ChildrenDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.CreateChildrenClientAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpGet("listChildrenByClient/{clientId}")]
        public async Task<IActionResult> ListChildrenClient(int clientId)
        {
            if (clientId == null || clientId == 0 )
                return BadRequest("Datos inválidos.");

            var childrens = await _clientService.ListChildrenClientAsync(clientId);

            return Ok(childrens);
        }

        [Authorize]
        [HttpPut("updateChild")]
        public async Task<IActionResult> UpdateChild([FromBody] UpdateChildrenDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.UpdateChildAsync(dto);

            if (!result)
                return NotFound("Hijo no encontrado.");

            return Ok(new { message = "Hijo actualizado correctamente." });

        }


        [Authorize]
        [HttpPost("children/{id}")]
        public async Task<IActionResult> UpdateChildrenClient(int id, [FromBody] ChildrenDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.UpdateChildrenClientAsync(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize]
        [HttpPost("updateClient/{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var result = await _clientService.UpdateClientAsync(id, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }


        [Authorize]
        [HttpPost("updateState/{id}")]
        public async Task<IActionResult> UpdateState(int id, [FromBody] UpdateStateRequest request)
        {
            if (request.IsActive == null)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var result = await _clientService.UpdateClientStateAsync(id, request.IsActive);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new { Success = true, Message = "Estado actualizado correctamente" });
        }

        [Authorize]
        [HttpPost("updateStateMarketing/{id}")]
        public async Task<IActionResult> updateStateMarketing(int id, [FromBody] UpdateStateMarketingRequest request)
        {
            if (request.acceptsMarketing == null)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var result = await _clientService.updateStateMarketing(id, request.acceptsMarketing);

            if (!result.Success)
                return BadRequest(new { Success = false, Message = result.Message });

            return Ok(new { Success = true, Message = "Estado actualizado correctamente" });
        }


        [Authorize]
        [HttpGet("childrenByClient")]
        public async Task<IActionResult> GetChildrenByClient()
        {
            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var children = await _clientService.GetChildrenByClient(establishmentId);
            if (children == null || !children.Any())
                return NotFound(new { Message = "No se encontraron empleados para este establecimiento" });

            return Ok(children);
        }

        [Authorize]
        [HttpGet("cliente/{documento}")]
        public async Task<IActionResult> ObtenerDescuento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return BadRequest("Debe proporcionar un documento válido.");

            int descuento = await _clientService.ObtenerDescuentoPorCliente(documento);

            return Ok(new
            {
                descuentoAplicado = descuento
            });
        }

        [Authorize]
        [HttpGet("reporte-clientes")]
        public async Task<IActionResult> ReporteClientes()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var establishmentId = int.Parse(User.FindFirst("establishmentId").Value);

            var clientes = await _clientService.GetClientByEstablishment(establishmentId);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Clientes");

            int row = 1;

            // CABECERA
            worksheet.Cells[row, 1].Value = "ID";
            worksheet.Cells[row, 2].Value = "Nombres";
            worksheet.Cells[row, 3].Value = "Tipo Documento";
            worksheet.Cells[row, 4].Value = "Nro Documento";
            worksheet.Cells[row, 5].Value = "Email";
            worksheet.Cells[row, 6].Value = "Teléfono Principal";
            worksheet.Cells[row, 7].Value = "Dirección Principal";
            worksheet.Cells[row, 8].Value = "Activo";
            worksheet.Cells[row, 9].Value = "Marketing";

            using (var range = worksheet.Cells[row, 1, row, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            row++;

            foreach (var cliente in clientes)
            {
                var telefonoPrincipal = cliente.Numbers?
                    .FirstOrDefault(x => x.IsPrimary)?.Number;

                var direccionPrincipal = cliente.Addresses?
                    .FirstOrDefault(x => x.IsPrimary)?.Address;

                worksheet.Cells[row, 1].Value = cliente.Id;
                worksheet.Cells[row, 2].Value = cliente.Names;
                worksheet.Cells[row, 3].Value = cliente.DocumentIdentificationType?.Name;
                worksheet.Cells[row, 4].Value = cliente.DocumentIdentificationNumber;
                worksheet.Cells[row, 5].Value = cliente.Email ?? "";
                worksheet.Cells[row, 6].Value = telefonoPrincipal ?? "";
                worksheet.Cells[row, 7].Value = direccionPrincipal ?? "";
                worksheet.Cells[row, 8].Value = cliente.IsActive ? "Sí" : "No";
                worksheet.Cells[row, 9].Value = cliente.AcceptsMarketing ? "Sí" : "No";

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            var nombreArchivo = $"ReporteClientes_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }

    }
}
