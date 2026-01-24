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
            services.AddScoped<VehicleCatalogService>();
            services.AddScoped<VehicleService>();
            services.AddScoped<VehicleIntakeService>();
            services.AddScoped<ServicesMasterService>();
            services.AddScoped<ProductService>();
            services.AddScoped<VehicleBudgetService>();
            services.AddScoped<WorkOrderService>();

            return services;
        }
    }
}
