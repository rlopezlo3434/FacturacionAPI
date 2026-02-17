using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Data
{
    public class SistemaVentasDbContext : DbContext
    {
        public SistemaVentasDbContext(DbContextOptions<SistemaVentasDbContext> options)
           : base(options)
        {
        }

        public DbSet<InventoryMasterItem> InventoryMasterItems { get; set; }
        public DbSet<VehicleIntake> VehicleIntakes { get; set; }
        public DbSet<VehicleIntakeInventoryItem> VehicleIntakeInventoryItems { get; set; }
        public DbSet<ServicesMaster> ServicesMasters { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<VehicleBudget> VehicleBudgets { get; set; }
        public DbSet<VehicleBudgetItem> VehicleBudgetItems { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderItem> WorkOrderItems { get; set; }
        public DbSet<VehicleIntakeDiagram> VehicleIntakeDiagram { get; set; }


        public DbSet<Companie> Companie { get; set; }
        public DbSet<Establishment> Establishment { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientNumbers> ClientNumbers { get; set; }
        public DbSet<ClientAddress> ClientAddresses { get; set; }
        public DbSet<UnitMeasure> UnitMeasure { get; set; }
        public DbSet<VehicleIntakeImage> VehicleIntakeImages { get; set; }

        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleOwner> VehicleOwners { get; set; }


        public DbSet<Item> Items { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<StockMovement> StockMovement { get; set; }
        public DbSet<ProductDefinition> ProductDefinition { get; set; }
        public DbSet<ChildrenClient> ChildrenClient { get; set; }
        public DbSet<Promotion> Promotion { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> ventaDetalles { get; set; }
        public DbSet<VentaEmpleado> ventaEmpleados { get; set; }
        public DbSet<AnulacionDocumento> AnulacionDocumento { get; set; }
        public DbSet<CajaApertura> CajaAperturas { get; set; }
        public DbSet<CajaMovimiento> CajaMovimientos { get; set; }
        public DbSet<CajaCierre> CajaCierres { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientNumbers>()
                .HasIndex(x => new { x.ClientId, x.IsPrimary })
                .IsUnique()
                .HasFilter("[IsPrimary] = 1");

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Brand)
                .WithMany()
                .HasForeignKey(v => v.BrandId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany()
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<VehicleModel>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Models)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ProductDefinition>()
                .Property(c => c.Item)
                .HasConversion<string>();

            modelBuilder.Entity< StockMovement>()
                .Property(c => c.MovementType)
                .HasConversion<string>();

            modelBuilder.Entity<Venta>()
                .Property(c => c.MetodoPago)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(x => x.VehicleIntake)
                .WithMany()
                .HasForeignKey(x => x.VehicleIntakeId)
                .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<WorkOrder>()
            //    .HasOne(x => x.Budget)
            //    .WithMany()
            //    .HasForeignKey(x => x.BudgetId)
            //    .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
