using System.Net;
using Framework.IntegrationTests.Infrastructure;

namespace Framework.IntegrationTests.Api;

public sealed class HealthEndpointTests : IClassFixture<FrameworkApiFactory>
{
    private readonly FrameworkApiFactory _factory;

    public HealthEndpointTests(FrameworkApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsSuccessOrServiceUnavailable()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/health");

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Expected 200 or 503 but received {(int)response.StatusCode}.");
    }
}

