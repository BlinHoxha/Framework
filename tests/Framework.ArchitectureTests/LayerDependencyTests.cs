using System.Reflection;

namespace Framework.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void AiCore_DoesNotReference_Framework_Or_ProviderAssemblies()
    {
        string[] references = GetAssemblyReferences(typeof(BuildingBlocks.AI.Core.AssemblyMarker).Assembly);

        Assert.DoesNotContain(references, reference => reference.StartsWith("Framework.", StringComparison.Ordinal));
        Assert.DoesNotContain(references, reference => reference.StartsWith("Azure.", StringComparison.Ordinal));
        Assert.DoesNotContain(references, reference => reference.StartsWith("AWSSDK.", StringComparison.Ordinal));
    }

    [Fact]
    public void LocalAiAdapter_References_Core_But_Not_Framework_Or_CloudProviders()
    {
        string[] references = GetAssemblyReferences(typeof(BuildingBlocks.AI.Local.AssemblyMarker).Assembly);

        Assert.Contains("BuildingBlocks.AI.Core", references);
        Assert.DoesNotContain(references, reference => reference.StartsWith("Framework.", StringComparison.Ordinal));
        Assert.DoesNotContain(references, reference => reference.StartsWith("Azure.", StringComparison.Ordinal));
        Assert.DoesNotContain(references, reference => reference.StartsWith("AWSSDK.", StringComparison.Ordinal));
    }

    [Fact]
    public void AzureAiAdapter_References_Core_But_Not_Framework()
    {
        string[] references = GetAssemblyReferences(typeof(BuildingBlocks.AI.Azure.AzureAiOptions).Assembly);

        Assert.Contains("BuildingBlocks.AI.Core", references);
        Assert.DoesNotContain(references, reference => reference.StartsWith("Framework.", StringComparison.Ordinal));
    }

    [Fact]
    public void Domain_DoesNotReference_Application_Api_Or_Infrastructure()
    {
        string[] references = GetAssemblyReferences(typeof(Framework.Domain.AssemblyMarker).Assembly);

        Assert.DoesNotContain("Framework.Application", references);
        Assert.DoesNotContain("Framework.Api", references);
        Assert.DoesNotContain("Framework.Infrastructure", references);
    }

    [Fact]
    public void Application_DoesNotReference_Api_Or_Infrastructure()
    {
        string[] references = GetAssemblyReferences(typeof(Framework.Application.AssemblyMarker).Assembly);

        Assert.DoesNotContain("Framework.Api", references);
        Assert.DoesNotContain("Framework.Infrastructure", references);
    }

    [Fact]
    public void Contracts_DoesNotReference_Domain_Application_Api_Or_Infrastructure()
    {
        string[] references = GetAssemblyReferences(typeof(Framework.Contracts.AssemblyMarker).Assembly);

        Assert.DoesNotContain("Framework.Domain", references);
        Assert.DoesNotContain("Framework.Application", references);
        Assert.DoesNotContain("Framework.Api", references);
        Assert.DoesNotContain("Framework.Infrastructure", references);
    }

    private static string[] GetAssemblyReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
}

