using FacturacionAPI.Services;

namespace FacturacionAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Aquí registras todos tus servicios
            services.AddScoped<EmployeeService>();
            services.AddScoped<ItemsService>();

            return services;
        }
    }
}
