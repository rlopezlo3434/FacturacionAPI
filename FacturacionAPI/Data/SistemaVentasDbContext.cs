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
        public DbSet<Items> Items { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<StockMovement> StockMovement { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Companie>()
               .Property(c => c.DocumentIdentificationType)
               .HasConversion<string>();

            modelBuilder.Entity<Employee>()
               .Property(c => c.Gender)
               .HasConversion<string>();

            modelBuilder.Entity<Employee>()
               .Property(c => c.DocumentIdentificationType)
               .HasConversion<string>();

            modelBuilder.Entity<Items>()
                .Property(c => c.Item)
                .HasConversion<string>();

            modelBuilder.Entity< StockMovement>()
                .Property(c => c.MovementType)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
