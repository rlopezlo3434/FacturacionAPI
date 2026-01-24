using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class WorkOrderService
    {
        private readonly SistemaVentasDbContext _context;

        public WorkOrderService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateWorkOrderFromOfficialBudgetAsync(int intakeId)
        {
            // ✅ buscar presupuesto oficial aprobado
            var budget = await _context.VehicleBudgets
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.VehicleIntakeId == intakeId &&
                    x.IsOfficial &&
                    x.IsApproved &&
                    x.IsActive);

            if (budget == null)
                return (false, "No existe un presupuesto oficial aprobado para este internamiento.");

            // ✅ opcional: evitar duplicados (una OT activa)
            var existsWO = await _context.WorkOrders
                .AnyAsync(x => x.VehicleIntakeId == intakeId && x.IsActive);

            if (existsWO)
                return (false, "Ya existe una Orden de Trabajo activa para este internamiento.");

            var code = await GenerateWorkOrderCodeAsync();

            var wo = new WorkOrder
            {
                Code = code,
                VehicleIntakeId = intakeId,
                BudgetId = budget.Id,
                Notes = budget.Notes, // puedes copiar o dejar null
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            foreach (var item in budget.Items)
            {
                wo.Items.Add(new WorkOrderItem
                {
                    ItemType = item.ItemType,
                    ProductId = item.ProductId,
                    ServiceMasterId = item.ServiceMasterId,
                    Quantity = item.Quantity,
                    IsCompleted = false
                });
            }

            _context.WorkOrders.Add(wo);
            await _context.SaveChangesAsync();

            return (true, $"Orden de Trabajo {code} generada correctamente.");
        }

        private async Task<string> GenerateWorkOrderCodeAsync()
        {
            // ✅ Buscar el último OT generado (mayor Id)
            var lastCode = await _context.WorkOrders
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                // lastCode ejemplo: OT-00025
                var parts = lastCode.Split("-");

                if (parts.Length == 2)
                {
                    var numText = parts[1]; // "00025"

                    if (int.TryParse(numText, out int lastNumber))
                        nextNumber = lastNumber + 1;
                }
            }

            return $"OT-{nextNumber:D5}";
        }

        public async Task<List<WorkOrderListDto>> GetWorkOrdersAsync()
        {
            return await _context.WorkOrders
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Vehicle).ThenInclude(v => v.Brand)
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Vehicle).ThenInclude(v => v.Model)
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Client)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new WorkOrderListDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    VehicleIntakeId = x.VehicleIntakeId,
                    Mode = (int)x.VehicleIntake.Mode,
                    CreatedAt = x.CreatedAt,
                    IsCompleted = x.Items.All(i => i.IsCompleted),

                    Vehicle = new VehicleMiniDto
                    {
                        Id = x.VehicleIntake.Vehicle.Id,
                        Plate = x.VehicleIntake.Vehicle.Plate,
                        Brand = new CatalogMiniDto
                        {
                            Id = x.VehicleIntake.Vehicle.Brand.Id,
                            Name = x.VehicleIntake.Vehicle.Brand.Name
                        },
                        Model = new CatalogMiniDto
                        {
                            Id = x.VehicleIntake.Vehicle.Model.Id,
                            Name = x.VehicleIntake.Vehicle.Model.Name
                        }
                    },

                    Client = new ClientMiniDto
                    {
                        Id = x.VehicleIntake.Client.Id,
                        Names = x.VehicleIntake.Client.Names
                    }
                })
                .ToListAsync();
        }

        // ✅ GET DETALLE POR ID
        public async Task<WorkOrderDetailDto?> GetWorkOrderDetailAsync(int id)
        {
            var wo = await _context.WorkOrders
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Vehicle).ThenInclude(v => v.Brand)
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Vehicle).ThenInclude(v => v.Model)
                .Include(x => x.VehicleIntake).ThenInclude(i => i.Client)
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .Include(x => x.Items).ThenInclude(i => i.ServiceMaster)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (wo == null) return null;

            return new WorkOrderDetailDto
            {
                Id = wo.Id,
                Code = wo.Code,
                VehicleIntakeId = wo.VehicleIntakeId,
                Mode = (int)wo.VehicleIntake.Mode,
                Notes = wo.Notes,
                CreatedAt = wo.CreatedAt,
                IsCompleted = wo.Items.All(i => i.IsCompleted),

                Vehicle = new VehicleMiniDto
                {
                    Id = wo.VehicleIntake.Vehicle.Id,
                    Plate = wo.VehicleIntake.Vehicle.Plate,
                    Brand = new CatalogMiniDto
                    {
                        Id = wo.VehicleIntake.Vehicle.Brand.Id,
                        Name = wo.VehicleIntake.Vehicle.Brand.Name
                    },
                    Model = new CatalogMiniDto
                    {
                        Id = wo.VehicleIntake.Vehicle.Model.Id,
                        Name = wo.VehicleIntake.Vehicle.Model.Name
                    }
                },

                Client = new ClientMiniDto
                {
                    Id = wo.VehicleIntake.Client.Id,
                    Names = wo.VehicleIntake.Client.Names
                },

                Items = wo.Items.Select(i => new WorkOrderItemDto
                {
                    Id = i.Id,
                    ItemType = (int)i.ItemType,
                    Quantity = i.Quantity,
                    IsCompleted = i.IsCompleted,
                    Observations = i.Observations,
                    Name = i.ItemType == BudgetItemType.Product
                        ? (i.Product != null ? i.Product.Name : "Producto")
                        : (i.ServiceMaster != null ? i.ServiceMaster.Name : "Servicio")
                }).ToList()
            };
        }

        // ✅ UPDATE CHECKLIST ITEMS
        public async Task<(bool Success, string Message)> UpdateWorkOrderItemsAsync(WorkOrderUpdateItemsDto dto)
        {
            var wo = await _context.WorkOrders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == dto.WorkOrderId);

            if (wo == null)
                return (false, "Orden de trabajo no encontrada.");

            if (dto.Items == null || dto.Items.Count == 0)
                return (false, "No se enviaron items para actualizar.");

            foreach (var item in dto.Items)
            {
                var dbItem = wo.Items.FirstOrDefault(x => x.Id == item.Id);
                if (dbItem == null) continue;

                dbItem.IsCompleted = item.IsCompleted;
                dbItem.Observations = item.Observations;
            }

            // ✅ si todos están completos, OT completada (opcional)
            wo.IsActive = true;

            await _context.SaveChangesAsync();
            return (true, "Orden de trabajo actualizada correctamente.");
        }
    }
}
