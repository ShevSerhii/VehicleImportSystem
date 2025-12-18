using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Infrastructure.Data;
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

        services.AddHttpClient();

        services.AddScoped<ICurrencyService, NbuCurrencyService>();
        services.AddScoped<IMarketPriceService, MockMarketPriceService>();
        services.AddHostedService<CurrencyRateWarmupService>();

        return services;
    }
}