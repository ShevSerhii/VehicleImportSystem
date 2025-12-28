using VehicleImportSystem.Infrastructure.Data;

namespace VehicleImportSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring the web application pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Initializes the database by applying migrations and seeding initial data.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await DbInitializer.InitializeAsync(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    /// <summary>
    /// Configures the middleware pipeline for the application.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void ConfigureMiddleware(this WebApplication app)
    {
        // Enable Swagger UI in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Configure middleware pipeline
        app.UseCors("AllowAngularApp");
        app.UseHttpsRedirection();
    }
}

