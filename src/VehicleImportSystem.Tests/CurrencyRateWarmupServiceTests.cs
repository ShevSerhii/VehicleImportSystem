using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Infrastructure.Services;

namespace VehicleImportSystem.Tests;

public class CurrencyRateWarmupServiceTests
{
    [Fact]
    public async Task StartAsync_WarmsUpRatesOnStartup()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var currency = new Mock<ICurrencyService>();
        currency.Setup(x => x.GetEuroRateAsync()).ReturnsAsync(42m);
        currency.Setup(x => x.GetUsdRateAsync())
            .ReturnsAsync(39m)
            .Callback(() => tcs.TrySetResult(true));

        using var provider = new ServiceCollection()
            .AddSingleton(currency.Object)
            .BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new CurrencyRateWarmupService(scopeFactory, NullLogger<CurrencyRateWarmupService>.Instance);

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);
        await Task.WhenAny(tcs.Task, Task.Delay(2000));
        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);

        tcs.Task.IsCompleted.Should().BeTrue();
        currency.Verify(x => x.GetEuroRateAsync(), Times.AtLeastOnce);
        currency.Verify(x => x.GetUsdRateAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WhenCurrencyServiceFails_DoesNotThrow()
    {
        var currency = new Mock<ICurrencyService>();
        currency.Setup(x => x.GetEuroRateAsync()).ThrowsAsync(new InvalidOperationException("fail"));

        using var provider = new ServiceCollection()
            .AddSingleton(currency.Object)
            .BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new CurrencyRateWarmupService(scopeFactory, NullLogger<CurrencyRateWarmupService>.Instance);

        using var cts = new CancellationTokenSource();
        var act = async () =>
        {
            await sut.StartAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();
            await sut.StopAsync(CancellationToken.None);
        };

        await act.Should().NotThrowAsync();
    }
}
