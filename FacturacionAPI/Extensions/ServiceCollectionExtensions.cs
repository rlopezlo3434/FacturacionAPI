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
            services.AddScoped<EstablishmentService>();
            services.AddScoped<ClientService>();
            services.AddScoped<PromotionService>();
            services.AddScoped<FacturacionService>();
            services.AddScoped<KardexService>();
            services.AddScoped<CajaService>();

            return services;
        }
    }
}
