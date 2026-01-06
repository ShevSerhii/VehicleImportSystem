using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;

namespace VehicleImportSystem.Infrastructure.Resilience;

/// <summary>
/// Configures Polly resilience policies for HTTP clients.
/// Provides retry, circuit breaker, and timeout policies for external API calls.
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Retry policy with exponential backoff for transient HTTP errors.
    /// Retries up to 3 times with increasing delays: 2s, 4s, 8s.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Logging is handled by the service using this policy
                });
    }

    /// <summary>
    /// Circuit breaker policy to prevent cascading failures.
    /// Opens circuit after 5 consecutive failures, stays open for 30 seconds.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    // Logging will be handled by the service using this policy
                },
                onReset: () =>
                {
                    // Logging will be handled by the service using this policy
                });
    }

    /// <summary>
    /// Timeout policy to prevent hanging requests.
    /// Times out after 45 seconds.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(45),
            timeoutStrategy: TimeoutStrategy.Pessimistic);
    }

    /// <summary>
    /// Combined policy with retry, circuit breaker, and timeout.
    /// Use this for external API calls that need all resilience features.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(
            GetTimeoutPolicy(),
            GetCircuitBreakerPolicy(),
            GetRetryPolicy());
    }
}

