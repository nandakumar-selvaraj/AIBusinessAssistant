using AiBusinessAssistant.Repositories.Implementations;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Implementations;
using AiBusinessAssistant.Services.Interfaces;
using AiBusinessAssistant.Features.AI;
using AiBusinessAssistant.Features.AI.Configuration;
using AiBusinessAssistant.Features.AI.Plugins;
using Microsoft.SemanticKernel;

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

            services.AddScoped<CustomerPlugin>();
            services.AddScoped<ProductPlugin>();
            services.AddScoped<OrderPlugin>();
            services.AddSingleton<ConversationLockProvider>();
            services.AddScoped<IBusinessAssistantService, BusinessAssistantService>();

            return services;
        }

        public static IServiceCollection AddBusinessAssistant(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<OllamaOptions>()
                .Bind(configuration.GetSection(OllamaOptions.SectionName))
                .Validate(options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                    "Ollama:Endpoint must be an absolute HTTP or HTTPS URL.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ModelId), "Ollama:ModelId is required.")
                .Validate(options => options.MaxHistoryMessages is >= 1 and <= 100, "Ollama:MaxHistoryMessages must be between 1 and 100.")
                .Validate(options => options.MaxMessageLength is >= 1 and <= 10000, "Ollama:MaxMessageLength must be between 1 and 10000.")
                .Validate(options => options.RequestTimeoutSeconds is >= 1 and <= 600, "Ollama:RequestTimeoutSeconds must be between 1 and 600.")
                .ValidateOnStart();

            var ollama = configuration.GetSection(OllamaOptions.SectionName).Get<OllamaOptions>() ?? new OllamaOptions();
#pragma warning disable SKEXP0070
            services.AddKernel().AddOllamaChatCompletion(
                modelId: ollama.ModelId,
                endpoint: new Uri(ollama.Endpoint));
#pragma warning restore SKEXP0070

            // Build one scoped Kernel per request so its plugins use the same scoped business services.
            services.AddScoped(sp =>
            {
                var kernel = new Kernel(sp);
                kernel.Plugins.AddFromObject(sp.GetRequiredService<CustomerPlugin>(), "Customer");
                kernel.Plugins.AddFromObject(sp.GetRequiredService<ProductPlugin>(), "Product");
                kernel.Plugins.AddFromObject(sp.GetRequiredService<OrderPlugin>(), "Order");
                return kernel;
            });

            return services;
        }
    }
}
