namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Standardized error response DTO for API error handling.
/// Provides consistent error format across all endpoints.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// HTTP status code of the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (optional, usually only in development).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// List of validation errors (if applicable).
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request path that caused the error.
    /// </summary>
    public string? Path { get; set; }
}

