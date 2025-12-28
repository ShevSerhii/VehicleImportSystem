using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleImportSystem.Domain.Settings;

namespace VehicleImportSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures application settings from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CustomsSettings>(
            configuration.GetSection("CustomsSettings"));
        
        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation for the API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }

    /// <summary>
    /// Configures JSON serialization options to use camelCase naming policy.
    /// This ensures compatibility with JavaScript/TypeScript frontend applications.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddCamelCaseJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        
        return services;
    }

    /// <summary>
    /// Configures CORS (Cross-Origin Resource Sharing) to allow requests from Angular frontend.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddCorsForAngularApp(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        
        return services;
    }
}

