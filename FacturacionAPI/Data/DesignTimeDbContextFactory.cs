using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FacturacionAPI.Data
{
    // Usado solo por las herramientas de EF Core (migrations) en tiempo de diseño,
    // para evitar levantar todo Program.cs (que carga la DLL nativa de wkhtmltopdf).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SistemaVentasDbContext>
    {
        public SistemaVentasDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SistemaVentasDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new SistemaVentasDbContext(optionsBuilder.Options);
        }
    }
}
