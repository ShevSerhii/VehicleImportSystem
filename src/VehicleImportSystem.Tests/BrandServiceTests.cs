using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Infrastructure.Services;
using VehicleImportSystem.Tests.Helpers;

namespace VehicleImportSystem.Tests;

public class BrandServiceTests
{
    [Fact]
    public async Task GetAllBrandsAsync_NoBrands_ReturnsEmptyCollection()
    {
        await using var dbContext = CreateDbContext();
        var sut = new BrandService(dbContext);

        var result = await sut.GetAllBrandsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllBrandsAsync_ReturnsBrandsOrderedByName()
    {
        await using var dbContext = CreateDbContext();
        dbContext.CarBrands.AddRange(
            new CarBrand { Id = 2, Name = "BMW" },
            new CarBrand { Id = 1, Name = "Audi" },
            new CarBrand { Id = 3, Name = "Volvo" });
        await dbContext.SaveChangesAsync();

        var sut = new BrandService(dbContext);

        var result = (await sut.GetAllBrandsAsync(CancellationToken.None)).ToList();

        result.Should().HaveCount(3);
        result.Select(x => x.Name).Should().ContainInOrder("Audi", "BMW", "Volvo");
        result.Select(x => x.Id).Should().ContainInOrder(1, 2, 3);
    }

    private static TestAppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestAppDbContext(options);
    }
}
