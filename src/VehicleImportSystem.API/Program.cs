using VehicleImportSystem.API.Endpoints;
using VehicleImportSystem.API.Extensions;
using VehicleImportSystem.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Configure application settings
builder.Services.AddApplicationSettings(builder.Configuration);

// Register all infrastructure services (DbContext, Services, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure API documentation
builder.Services.AddApiDocumentation();

// Configure JSON serialization (camelCase for frontend compatibility)
builder.Services.AddCamelCaseJsonOptions();

// Configure CORS for Angular frontend
builder.Services.AddCorsForAngularApp();

var app = builder.Build();

// Initialize database (migrations + seeding)
await app.InitializeDatabaseAsync();

// Configure middleware pipeline
app.ConfigureMiddleware();

// Map API endpoints
app.MapCalculatorEndpoints();
app.MapCurrencyEndpoints();
app.MapBrandEndpoints();
app.MapHistoryEndpoints();
app.MapMarketEndpoints();

app.Run();