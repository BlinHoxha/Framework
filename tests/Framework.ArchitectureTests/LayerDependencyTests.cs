using System.Reflection;

namespace Framework.ArchitectureTests;

public sealed class LayerDependencyTests
{
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

