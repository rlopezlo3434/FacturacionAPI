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
                //BudgetId = budget.Id,
                Notes = budget.Notes, // puedes copiar o dejar null
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            foreach (var item in budget.Items)
            {
                //wo.Items.Add(new WorkOrderItem
                //{
                //    ItemType = item.ItemType,
                //    ProductId = item.ProductId,
                //    ServiceMasterId = item.ServiceMasterId,
                //    Quantity = item.Quantity,
                //    IsCompleted = false
                //});
            }

            _context.WorkOrders.Add(wo);
            await _context.SaveChangesAsync();

            return (true, $"Orden de Trabajo {code} generada correctamente.");
        }

        public async Task<(bool Success, string Message)> GenerateOrUpdateWorkOrderAsync(int intakeId)
        {
            var idInter= await _context.VehicleIntakes
                .Where(x => x.Correlativo == intakeId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var intakeExists = await _context.VehicleIntakes.AnyAsync(x => x.Correlativo == intakeId);
            if (!intakeExists)
                return (false, "Internamiento no existe.");

            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(x => x.VehicleIntakeId == idInter && x.IsActive);

            if (workOrder == null)
            {
                workOrder = new WorkOrder
                {
                    Code = await GenerateWorkOrderCodeAsync(),
                    VehicleIntakeId = idInter,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.WorkOrders.Add(workOrder);
                await _context.SaveChangesAsync();
            }

            // IDs de todos los budget items aprobados del internamiento
            var approvedBudgetItemIds = await _context.VehicleBudgetItems
                .Where(x => x.VehicleBudget.VehicleIntakeId == idInter && x.IsApproved)
                .Select(x => x.Id)
                .ToListAsync();

            // WorkOrderItems actuales de esta OT
            var currentWorkOrderItems = await _context.WorkOrderItems
                .Where(x => x.WorkOrderId == workOrder.Id)
                .ToListAsync();

            // Eliminar los que ya no están aprobados
            var toRemove = currentWorkOrderItems
                .Where(w => !approvedBudgetItemIds.Contains(w.VehicleBudgetItemId))
                .ToList();

            _context.WorkOrderItems.RemoveRange(toRemove);

            // Agregar los aprobados que aún no están en la OT
            var existingIds = currentWorkOrderItems
                .Select(w => w.VehicleBudgetItemId)
                .ToHashSet();

            var toAdd = approvedBudgetItemIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new WorkOrderItem
                {
                    WorkOrderId = workOrder.Id,
                    VehicleBudgetItemId = id
                });

            _context.WorkOrderItems.AddRange(toAdd);

            await _context.SaveChangesAsync();

            return (true, $"Orden de Trabajo {workOrder.Code} sincronizada con {approvedBudgetItemIds.Count} ítems aprobados.");
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
                    Correlativo = x.VehicleIntake.Correlativo,
                    Mode = (int)x.VehicleIntake.Mode,
                    CreatedAt = x.CreatedAt,
                    //IsCompleted = x.Items.All(i => i.IsCompleted),

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
                .Include(x => x.WorkOrderEmployees).ThenInclude(x => x.Employee)
                .Include(x => x.WorkOrderSuppliers).ThenInclude(x => x.Proveedor)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (wo == null) return null;

            var empleados = wo.WorkOrderEmployees
                .Select(x => new EmployeeMiniDto
                {
                    Id = x.Employee.Id,
                    Names = x.Employee.Names
                })
                .ToList();

            var proveedores = wo.WorkOrderSuppliers
                .Select(x => new ProveedorMiniDto
                {
                    Id = x.Proveedor.Id,
                    Ruc = x.Proveedor.Ruc,
                    RazonSocial = x.Proveedor.RazonSocial
                })
                .ToList();

            // ✅ ITEMS DE OT = ITEMS APROBADOS DE PRESUPUESTOS
            var items = await _context.WorkOrderItems
                        .Where(x => x.WorkOrderId == wo.Id)
                        .Select(x => new WorkOrderItemDto
                        {
                            Id = x.Id,
                            ItemType = (int)x.VehicleBudgetItem.ItemType,
                            BudgetItemId = x.VehicleBudgetItemId,
                            Name = x.VehicleBudgetItem.ProductId != null
                                ? x.VehicleBudgetItem.Product!.Name
                                : x.VehicleBudgetItem.ServiceMaster!.Name,
                            Quantity = x.VehicleBudgetItem.Quantity,
                            IsCompleted = x.IsCompleted,
                            Observations = x.Observations
                        })
                        .ToListAsync();

            return new WorkOrderDetailDto
            {
                Id = wo.Id,
                Code = wo.Code,
                VehicleIntakeId = wo.VehicleIntakeId,
                Mode = (int)wo.VehicleIntake.Mode,
                CreatedAt = wo.CreatedAt,
                Notes = wo.Notes,
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

                Items = items,
                Employees = empleados,

                Proveedores = proveedores
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

            var empleadosActuales = await _context.WorkOrderEmployees
                                                  .Where(x => x.WorkOrderId == dto.WorkOrderId)
                                                  .ToListAsync();

            _context.WorkOrderEmployees.RemoveRange(empleadosActuales);

            if (dto.EmpleadosIds != null && dto.EmpleadosIds.Any())
            {
                var nuevosEmpleados = dto.EmpleadosIds.Select(empId => new WorkOrderEmployee
                {
                    WorkOrderId = dto.WorkOrderId,
                    EmployeeId = empId
                });

                await _context.WorkOrderEmployees.AddRangeAsync(nuevosEmpleados);
            }

            // =========================
            // PROVEEDORES
            // =========================

            var proveedoresActuales = await _context.WorkOrderSuppliers
                                                    .Where(x => x.WorkOrderId == dto.WorkOrderId)
                                                    .ToListAsync();

            _context.WorkOrderSuppliers.RemoveRange(proveedoresActuales);

            if (dto.ProveedoresIds != null && dto.ProveedoresIds.Any())
            {
                var nuevosProveedores = dto.ProveedoresIds.Select(provId => new WorkOrderSupplier
                {
                    WorkOrderId = dto.WorkOrderId,
                    ProveedorId = provId
                });

                await _context.WorkOrderSuppliers.AddRangeAsync(nuevosProveedores);
            }


            // ✅ opcional: cerrar OT si todos están completos
            if (wo.Items.Any() && wo.Items.All(x => x.IsCompleted))
            {
                wo.IsActive = false; // OT finalizada
            }

            // ✅ si todos están completos, OT completada (opcional)
            wo.IsActive = true;

            await _context.SaveChangesAsync();
            return (true, "Orden de trabajo actualizada correctamente.");
        }
    }
}
