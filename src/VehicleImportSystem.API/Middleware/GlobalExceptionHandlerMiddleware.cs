using System.Net;
using System.Text.Json;
using VehicleImportSystem.Application.DTOs;
using FluentValidation;

namespace VehicleImportSystem.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions and returns standardized error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponseDto
        {
            Path = context.Request.Path,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Input validation errors.";
                response.Errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case ArgumentException argException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argException.Message;
                if (_environment.IsDevelopment())
                {
                    response.Details = argException.StackTrace;
                }
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Requested resource not found.";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Access denied.";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An internal server error occurred. Please try again later.";
                if (_environment.IsDevelopment())
                {
                    response.Details = exception.StackTrace;
                }
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}

