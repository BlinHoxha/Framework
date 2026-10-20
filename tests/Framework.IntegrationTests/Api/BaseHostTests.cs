using System.Net;
using Framework.IntegrationTests.Infrastructure;
namespace Framework.IntegrationTests.Api;
public sealed class BaseHostTests
{
    [Fact]
    public async Task Base_host_runs_without_ai_and_does_not_publish_ai_routes()
    {
        using var factory = new FrameworkApiFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/ai/document-profiles")).StatusCode);
    }
}
