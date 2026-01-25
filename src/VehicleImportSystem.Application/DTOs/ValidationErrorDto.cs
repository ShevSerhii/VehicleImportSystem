namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object for validation errors.
/// Used when FluentValidation fails.
/// </summary>
public class ValidationErrorDto
{
    /// <summary>
    /// Error message describing the validation failure.
    /// </summary>
    public string Message { get; set; } = "Input validation errors";

    /// <summary>
    /// Dictionary of validation errors grouped by property name.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

