using FluentValidation;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Application.Validators;

public class CalculationRequestValidator : AbstractValidator<CalculationRequest>
{
    private const int MinYear = 1996; // Euro-2 standard introduction
    private const int MinCapacity = 1;
    private const int MaxIceCapacityCc = 10000;
    private const int MaxElectricCapacityKwh = 300;
    private const decimal MinPrice = 100m;
    private const decimal MaxPrice = 10_000_000m;

    public CalculationRequestValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(MinYear)
            .WithMessage($"Vehicle must meet Euro-2 standards (manufactured after {MinYear}).")
            .LessThanOrEqualTo(x => DateTime.UtcNow.Year + 1)
            .WithMessage("Year of manufacture cannot be in the future.");

        RuleFor(x => x.FuelType)
            .IsInEnum()
            .WithMessage("Invalid fuel type. Permitted values: Petrol, Diesel, Electric, Hybrid.");

        RuleFor(x => x.EngineCapacity)
            .GreaterThanOrEqualTo(MinCapacity)
            .WithMessage($"Capacity must be greater than or equal to {MinCapacity}.");

        RuleFor(x => x.PriceInEur)
            .GreaterThan(MinPrice)
            .WithMessage($"Price must be greater than {MinPrice} €.")
            .LessThanOrEqualTo(MaxPrice)
            .WithMessage($"Price cannot be greater than {MaxPrice} €.");

        RuleFor(x => x.MarkId)
            .GreaterThan(0)
            .WithMessage("Brand is required.");

        RuleFor(x => x.ModelId)
            .GreaterThan(0)
            .WithMessage("Model is required.");

        // Electric vehicles validation
        When(x => x.FuelType == FuelType.Electric, () =>
        {
            RuleFor(x => x.EngineCapacity)
                .LessThanOrEqualTo(MaxElectricCapacityKwh)
                .WithMessage($"For electric vehicles, battery capacity cannot exceed {MaxElectricCapacityKwh} kWh.");
        });

        // ICE and Hybrid vehicles validation
        When(x => x.FuelType == FuelType.Petrol ||
             x.FuelType == FuelType.Diesel ||
             x.FuelType == FuelType.Hybrid, () =>
             {
                 RuleFor(x => x.EngineCapacity)
                     .LessThanOrEqualTo(MaxIceCapacityCc)
                     .WithMessage($"For passenger vehicles, engine capacity cannot exceed {MaxIceCapacityCc} cm³.");
             });
    }
}