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

        public DbSet<Companie> Companie { get; set; }
        public DbSet<Establishment> Establishment { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientNumbers> ClientNumbers { get; set; }
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
        public DbSet<CodigosUtilizados> CodigosUtilizados { get; set; }
        public DbSet<VisitaCliente> VisitaClientes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Companie>()
               .Property(c => c.DocumentIdentificationType)
               .HasConversion<string>();

            modelBuilder.Entity<Establishment>()
               .Property(c => c.DocumentIdentificationType)
               .HasConversion<string>();

            modelBuilder.Entity<Employee>()
               .Property(c => c.Gender)
               .HasConversion<string>();

            modelBuilder.Entity<Employee>()
               .Property(c => c.DocumentIdentificationType)
               .HasConversion<string>();

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
        }
    }
}
