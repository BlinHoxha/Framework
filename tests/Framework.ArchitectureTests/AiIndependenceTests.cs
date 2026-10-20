using System.Reflection;
namespace Framework.ArchitectureTests;
public sealed class AiIndependenceTests
{
    [Theory]
    [InlineData("Framework.Domain")]
    [InlineData("Framework.Contracts")]
    [InlineData("Framework.Application")]
    [InlineData("Framework.Infrastructure")]
    [InlineData("Framework.Web")]
    [InlineData("Framework.Api")]
    public void Base_projects_do_not_reference_ai_or_cloud_ai_sdks(string name)
    {
        var names = Assembly.Load(name).GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        Assert.DoesNotContain(names, n => n.StartsWith("AIFramework") || n.StartsWith("BuildingBlocks.AI") || n.StartsWith("AWSSDK.Bedrock") || n == "Azure.Identity");
    }
}
