using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Background service: warms up EUR and USD rates on startup and then once per day via ICurrencyService.
/// </summary>
public class CurrencyRateWarmupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurrencyRateWarmupService> _logger;

    public CurrencyRateWarmupService(IServiceScopeFactory scopeFactory, ILogger<CurrencyRateWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WarmUp(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                await WarmUp(stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task WarmUp(CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

        try
        {
            var eurRate = await currencyService.GetEuroRateAsync();
            var usdRate = await currencyService.GetUsdRateAsync();

            _logger.LogInformation("Currency warmup complete. EUR rate: {EurRate}, USD rate: {UsdRate}", eurRate, usdRate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Currency warmup failed; will retry in next scheduled run.");
        }
    }
}