using System.Net;
using System.Net.Http;
using Moq;

namespace VehicleImportSystem.Tests.Helpers;

internal sealed class DelegatingStubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public int CallCount { get; private set; }

    public DelegatingStubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        return await _handler(request, cancellationToken);
    }
}

internal static class HttpClientFactoryMock
{
    public static IHttpClientFactory Create(HttpClient httpClient)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return factory.Object;
    }
}
