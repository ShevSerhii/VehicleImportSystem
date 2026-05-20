using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleImportSystem.API.Middleware;

namespace VehicleImportSystem.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsBadRequestWithErrors()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/calculator/calculate";
        context.Response.Body = new MemoryStream();

        var exception = new ValidationException(new List<ValidationFailure>
        {
            new("PriceInEur", "Price is required"),
            new("PriceInEur", "Price must be positive")
        });

        var sut = new GlobalExceptionHandlerMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionHandlerMiddleware>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Production });

        await sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        body.Should().Contain("Input validation errors");
        body.Should().Contain("PriceInEur");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_InProduction_HidesInternalDetails()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/fail";
        context.Response.Body = new MemoryStream();

        var sut = new GlobalExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("sensitive details"),
            NullLogger<GlobalExceptionHandlerMiddleware>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Production });

        await sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        body.Should().Contain("An internal server error occurred");
        body.Should().NotContain("sensitive details");
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_InDevelopment_ReturnsMessageAndDetails()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/fail";
        context.Response.Body = new MemoryStream();

        var sut = new GlobalExceptionHandlerMiddleware(
            _ => throw new ArgumentException("Bad argument"),
            NullLogger<GlobalExceptionHandlerMiddleware>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Development });

        await sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.GetProperty("message").GetString().Should().Be("Bad argument");
        doc.RootElement.GetProperty("details").GetString().Should().NotBeNullOrWhiteSpace();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment, IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
    }
}
