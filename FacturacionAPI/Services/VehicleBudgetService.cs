using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class VehicleBudgetService
    {
        private readonly SistemaVentasDbContext _context;

        public VehicleBudgetService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        // ✅ listar presupuestos por intake
        public async Task<List<VehicleBudgetListDto>> GetBudgetsByIntakeAsync(int intakeId)
        {
            return await _context.VehicleBudgets
                .Where(x => x.VehicleIntakeId == intakeId && x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new VehicleBudgetListDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    IsApproved = x.IsApproved,
                    IsOfficial = x.IsOfficial,
                    Total = x.Total,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // ✅ crear presupuesto
        public async Task<(bool Success, string Message)> CreateBudgetAsync(VehicleBudgetCreateDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return (false, "El presupuesto debe tener al menos 1 item.");

            var intakeExists = await _context.VehicleIntakes.AnyAsync(x => x.Id == dto.VehicleIntakeId);
            if (!intakeExists)
                return (false, "El internamiento no existe.");

            var code = await GenerateBudgetCodeAsync(dto.VehicleIntakeId);

            decimal subtotal = 0;

            var budget = new VehicleBudget
            {
                Code = code,
                VehicleIntakeId = dto.VehicleIntakeId,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
                IsApproved = false,
                IsOfficial = false
            };

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    return (false, "Cantidad inválida.");

                if (item.UnitPrice < 0)
                    return (false, "Precio inválido.");

                var totalItem = item.UnitPrice * item.Quantity;
                subtotal += totalItem;

                budget.Items.Add(new VehicleBudgetItem
                {
                    ItemType = (BudgetItemType)item.ItemType,
                    ProductId = item.ProductId,
                    ServiceMasterId = item.ServiceMasterId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = totalItem
                });
            }

            budget.SubTotal = subtotal;
            budget.Total = subtotal; // ✅ luego puedes agregar IGV, descuento, etc

            _context.VehicleBudgets.Add(budget);
            await _context.SaveChangesAsync();

            return (true, "Presupuesto creado correctamente.");
        }

        // ✅ aprobar presupuesto (vuelve oficial)
        public async Task<(bool Success, string Message)> ApproveBudgetAsync(int budgetId)
        {
            var budget = await _context.VehicleBudgets
                .FirstOrDefaultAsync(x => x.Id == budgetId);

            if (budget == null)
                return (false, "Presupuesto no encontrado.");

            // ✅ Quitar oficialidad a los anteriores del mismo intake
            var others = await _context.VehicleBudgets
                .Where(x => x.VehicleIntakeId == budget.VehicleIntakeId)
                .ToListAsync();

            //foreach (var b in others)
            //{
            //    b.IsOfficial = false;
            //}

            // ✅ Marcar el actual como aprobado y oficial
            budget.IsApproved = true;
            budget.IsOfficial = true;
            budget.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, "Presupuesto aprobado correctamente.");
        }

        // ✅ generar código único B0001
        private async Task<string> GenerateBudgetCodeAsync()
        {
            var last = await _context.VehicleBudgets
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(last))
                return "B0001";

            var numberPart = last.Replace("B", "");
            if (!int.TryParse(numberPart, out var lastNumber))
                return "B0001";

            var newNumber = lastNumber + 1;
            return $"B{newNumber:D4}";
        }

        private async Task<string> GenerateBudgetCodeAsync(int intakeId)
        {
            var last = await _context.VehicleBudgets
                .Where(x => x.VehicleIntakeId == intakeId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            int newNumber = 1;

            if (!string.IsNullOrEmpty(last))
            {
                // last puede ser INT0005-B0003
                var parts = last.Split("-B");
                if (parts.Length == 2 && int.TryParse(parts[1], out var lastNumber))
                    newNumber = lastNumber + 1;
            }

            return $"INT{intakeId:D4}-B{newNumber:D4}";
        }

        public async Task<VehicleBudgetDetailDto?> GetBudgetDetailAsync(int budgetId)
        {
            return await _context.VehicleBudgets
                .Where(x => x.Id == budgetId)
                .Select(x => new VehicleBudgetDetailDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    IsApproved = x.IsApproved,
                    IsOfficial = x.IsOfficial,
                    Notes = x.Notes,
                    SubTotal = x.SubTotal,
                    Total = x.Total,
                    CreatedAt = x.CreatedAt,
                    Items = x.Items.Select(i => new VehicleBudgetItemDetailDto
                    {
                        Id = i.Id,
                        ItemType = (int)i.ItemType,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice,
                        Product = i.ProductId == null ? null : new CatalogItemDto
                        {
                            Id = i.Product!.Id,
                            Name = i.Product.Name
                        },
                        Service = i.ServiceMasterId == null ? null : new CatalogItemDto
                        {
                            Id = i.ServiceMaster!.Id,
                            Name = i.ServiceMaster.Name
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    }
}
