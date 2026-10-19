using AiBusinessAssistant.Repositories.Implementations;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Implementations;
using AiBusinessAssistant.Services.Interfaces;

namespace AiBusinessAssistant.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
