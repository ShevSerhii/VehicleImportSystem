using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Infrastructure.Services;
using VehicleImportSystem.Tests.Helpers;

namespace VehicleImportSystem.Tests;

public class HistoryServiceTests
{
    [Fact]
    public async Task GetUserHistoryAsync_ReturnsOnlyUserRecords_OrderedByDateDesc()
    {
        await using var dbContext = CreateDbContext();
        SeedHistory(dbContext);
        var sut = new HistoryService(dbContext);

        var result = await sut.GetUserHistoryAsync("device-a", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().ContainInOrder(2, 1);
        result.All(x => x.BrandName == "Brand-1").Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRecordAsync_RecordExists_RemovesAndReturnsTrue()
    {
        await using var dbContext = CreateDbContext();
        SeedHistory(dbContext);
        var sut = new HistoryService(dbContext);

        var deleted = await sut.DeleteRecordAsync(1, CancellationToken.None);

        deleted.Should().BeTrue();
        dbContext.CustomsCalculation.Any(x => x.Id == 1).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRecordAsync_RecordMissing_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();
        SeedHistory(dbContext);
        var sut = new HistoryService(dbContext);

        var deleted = await sut.DeleteRecordAsync(999, CancellationToken.None);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task ClearUserHistoryAsync_RemovesOnlyCurrentUserRecords()
    {
        await using var dbContext = CreateDbContext();
        SeedHistory(dbContext);
        var sut = new HistoryService(dbContext);

        await sut.ClearUserHistoryAsync("device-a", CancellationToken.None);

        dbContext.CustomsCalculation.Count().Should().Be(1);
        dbContext.CustomsCalculation.Single().UserDeviceId.Should().Be("device-b");
    }

    private static TestAppDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestAppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static void SeedHistory(TestAppDbContext dbContext)
    {
        dbContext.CarBrands.Add(new CarBrand { Id = 1, Name = "Brand-1" });
        dbContext.CarModels.Add(new CarModel { Id = 10, BrandId = 1, Name = "Model-10" });

        dbContext.CustomsCalculation.AddRange(
            new CustomsCalculation
            {
                Id = 1,
                UserDeviceId = "device-a",
                BrandId = 1,
                ModelId = 10,
                Year = 2018,
                PriceInEur = 10000m,
                TotalTurnkeyPrice = 14000m,
                PotentialProfit = 1000m,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new CustomsCalculation
            {
                Id = 2,
                UserDeviceId = "device-a",
                BrandId = 1,
                ModelId = 10,
                Year = 2019,
                PriceInEur = 11000m,
                TotalTurnkeyPrice = 15000m,
                PotentialProfit = 1200m,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new CustomsCalculation
            {
                Id = 3,
                UserDeviceId = "device-b",
                BrandId = 1,
                ModelId = 10,
                Year = 2020,
                PriceInEur = 12000m,
                TotalTurnkeyPrice = 16000m,
                PotentialProfit = 1500m,
                CreatedAt = DateTime.UtcNow
            });

        dbContext.SaveChanges();
    }
}
