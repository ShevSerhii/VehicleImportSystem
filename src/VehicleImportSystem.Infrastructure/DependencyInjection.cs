using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly.Extensions.Http;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Infrastructure.Data;
using VehicleImportSystem.Infrastructure.Resilience;
using VehicleImportSystem.Infrastructure.Services;

namespace VehicleImportSystem.Infrastructure;

/// <summary>
/// Extension method to register Infrastructure services.
/// Keeps Program.cs clean by encapsulating database and service registration logic.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Database Context, Repositories, and External Services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<ICustomsCalculatorService, CustomsCalculatorService>();

        // Configure HTTP clients with Polly resilience policies
        services.AddHttpClient();
        
        // Configure NBU API client with resilience policies
        services.AddHttpClient("NbuApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy());

        // Configure Auto.ria API client with resilience policies
        services.AddHttpClient("AutoRiaApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy());

        services.AddScoped<ICurrencyService, NbuCurrencyService>();
        services.AddScoped<IMarketPriceService, AutoRiaMarketPriceService>();
        services.AddHostedService<CurrencyRateWarmupService>();

        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IHistoryService, HistoryService>();

        return services;
    }
}