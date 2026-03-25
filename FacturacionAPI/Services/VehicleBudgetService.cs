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

                if (item.Discount < 0)
                    return (false, "Descuento inválido.");

                var gross = item.UnitPrice * item.Quantity;

                if (item.Discount > gross)
                    return (false, "El descuento no puede ser mayor al total del ítem.");

                var totalItem = gross - item.Discount;

                subtotal += totalItem;

                budget.Items.Add(new VehicleBudgetItem
                {
                    ItemType = (BudgetItemType)item.ItemType,
                    ProductId = item.ProductId,
                    ServiceMasterId = item.ServiceMasterId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,   
                    TotalPrice = totalItem,
                    IsApproved = false,
                    ServicePackageId = item.ServicePackageId
                });
            }

            budget.SubTotal = subtotal;
            budget.Total = subtotal; // ✅ luego puedes agregar IGV, descuento, etc

            _context.VehicleBudgets.Add(budget);
            await _context.SaveChangesAsync();

            return (true, "Presupuesto creado correctamente.");
        }

        public async Task<(bool Success, string Message)> ApproveBudgetItemsAsync(
    BudgetApprovalRequestDto dto)
        {
            var budget = await _context.VehicleBudgets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == dto.BudgetId && b.IsActive);

            if (budget == null)
                return (false, "Presupuesto no encontrado.");

            decimal totalApproved = 0;

            foreach (var itemDto in dto.Items)
            {
                var item = budget.Items.FirstOrDefault(x => x.Id == itemDto.ItemId);
                if (item == null) continue;

                // ✅ actualizar valores editables
                item.Quantity = itemDto.Quantity;
                item.UnitPrice = itemDto.UnitPrice;
                item.IsApproved = itemDto.IsApproved;

                var gross = item.Quantity * item.UnitPrice;

                var discount = itemDto.Discount < 0 ? 0 : itemDto.Discount;
                if (discount > gross) discount = gross;

                item.Discount = discount;
                item.TotalPrice = gross - discount;

                if (item.IsApproved)
                {
                    totalApproved += item.TotalPrice;
                }
            }

            // ✅ total solo de items aprobados
            budget.Total = totalApproved;

            // opcional: aprobado si hay al menos 1 item aprobado
            budget.IsApproved = budget.Items.Any(i => i.IsApproved);

            await _context.SaveChangesAsync();

            return (true, "Aprobaciones guardadas correctamente.");
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
                .Include(x => x.VehicleIntake)
                    .ThenInclude(vi => vi.Client)
                .Include(x => x.VehicleIntake)
                    .ThenInclude(vi => vi.Vehicle)
                        .ThenInclude(v => v.Brand)
                .Include(x => x.VehicleIntake)
                    .ThenInclude(vi => vi.Vehicle)
                        .ThenInclude(v => v.Model)
                .Include(x => x.Items)
                .ThenInclude(i => i.ServicePackage)
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
                    VehicleIntake = new VehicleIntakeDetailDto2
                    {
                        Id = x.VehicleIntake.Id,
                        MileageKm = x.VehicleIntake.MileageKm,
                        Observations = x.VehicleIntake.Observations,
                        Services = x.VehicleIntake.Services,
                        CreatedAt = x.VehicleIntake.CreatedAt,
                        Client = new Client
                        {
                            Id = x.VehicleIntake.Client.Id,
                            Names = x.VehicleIntake.Client.Names,
                            DocumentIdentificationNumber = x.VehicleIntake.Client.DocumentIdentificationNumber,
                            Email = x.VehicleIntake.Client.Email,
                            Numbers = x.VehicleIntake.Client.Numbers,
                            Addresses = x.VehicleIntake.Client.Addresses,
                        },
                        Vehicle = new VehicleDto
                        {
                            Id = x.VehicleIntake.Vehicle.Id,
                            Plate = x.VehicleIntake.Vehicle.Plate,
                            SerialNumber = x.VehicleIntake.Vehicle.SerialNumber,
                            Vin = x.VehicleIntake.Vehicle.Vin,
                            Year = x.VehicleIntake.Vehicle.Year,
                            Color = x.VehicleIntake.Vehicle.Color,
                            Brand = new BrandDto
                            {
                                Id = x.VehicleIntake.Vehicle.Brand.Id,
                                Name = x.VehicleIntake.Vehicle.Brand.Name
                            },
                            Model = new ModelDto
                            {
                                Id = x.VehicleIntake.Vehicle.Model.Id,
                                Name = x.VehicleIntake.Vehicle.Model.Name
                            }
                        }
                    },
                    Items = x.Items.Select(i => new VehicleBudgetItemDetailDto
                    {
                        Id = i.Id,
                        ItemType = (int)i.ItemType,
                        Quantity = i.Quantity,
                        Discount = i.Discount,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice,
                        IsApproved = i.IsApproved,
                        Product = i.ProductId == null ? null : new CatalogItemDto
                        {
                            Id = i.Product!.Id,
                            Name = i.Product.Name
                        },
                        Service = i.ServiceMasterId == null ? null : new CatalogItemDto
                        {
                            Id = i.ServiceMaster!.Id,
                            Name = i.ServiceMaster.Name
                        },
                        ServicePackage = i.ServicePackageId == null ? null : new CatalogItemDto
                        {
                            Id = i.ServicePackage!.Id,
                            Name = i.ServicePackage.Description
                        },
                        ServicePackageId = i.ServicePackageId
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    }
}
