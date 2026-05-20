using FluentAssertions;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Validators;
using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Tests;

public class CalculationRequestValidatorTests
{
    private readonly CalculationRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PriceInEurIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PriceInEur = -1m;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.PriceInEur));
    }

    [Fact]
    public void Validate_EngineCapacityIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EngineCapacity = -100;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.EngineCapacity));
    }

    [Fact]
    public void Validate_YearIsUnrealistic_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Year = 1800;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.Year));
    }

    [Fact]
    public void Validate_EvVatExemptShareIsOutOfRange_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EvVatExemptShare = 1.1m;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.EvVatExemptShare));
    }

    [Fact]
    public void Validate_ElectricCapacityTooHigh_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FuelType = FuelType.Electric;
        request.EngineCapacity = 301;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.EngineCapacity));
    }

    [Fact]
    public void Validate_HybridWithoutExciseScheme_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FuelType = FuelType.Hybrid;
        request.HybridExciseScheme = null;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.HybridExciseScheme));
    }

    [Fact]
    public void Validate_HybridByIceEngineWithInvalidFuelType_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FuelType = FuelType.Hybrid;
        request.HybridExciseScheme = HybridExciseScheme.ByIceEngine;
        request.HybridIceFuelType = FuelType.Electric;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CalculationRequest.HybridIceFuelType));
    }

    private static CalculationRequest CreateValidRequest()
    {
        return new CalculationRequest
        {
            MarkId = 1,
            ModelId = 10,
            Year = DateTime.UtcNow.Year - 3,
            FuelType = FuelType.Petrol,
            EngineCapacity = 2000,
            PriceInEur = 12000m,
            EvVatExemptShare = 0m
        };
    }
}
