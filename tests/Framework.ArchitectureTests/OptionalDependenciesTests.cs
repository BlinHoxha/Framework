using System.Reflection;

namespace Framework.ArchitectureTests;

public sealed class OptionalDependenciesTests
{
    [Theory]
    [InlineData("Framework.Application.Abstractions.Persistence.IBaseRepository`2")]
    [InlineData("Framework.Application.Abstractions.Services.IBaseService`2")]
    [InlineData("Framework.Application.Services.BaseService`2")]
    [InlineData("Framework.Application.Mapping.ApplicationMappingProfile")]
    [InlineData("Framework.Application.DependencyInjection")]
    public void Original_application_assembly_resolves_forwarded_public_types(string name)
    {
        var resolved = Type.GetType($"{name}, Framework.Application", throwOnError: true)!;
        Assert.Equal(name, resolved.FullName);
        Assert.NotEqual("Framework.Application", resolved.Assembly.GetName().Name);
    }

    [Fact]
    public void Lean_persistence_has_no_mapping_or_database_provider_dependency()
    {
        var references = typeof(Framework.Persistence.FrameworkDbContext).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("AutoMapper", references);
        Assert.DoesNotContain("Framework.Application", references);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.SqlServer", references);
        Assert.DoesNotContain("Npgsql.EntityFrameworkCore.PostgreSQL", references);
    }

    [Fact]
    public void Legacy_context_keeps_its_original_public_name_and_extends_lean_context()
    {
        Assert.Equal("Framework.Infrastructure.Persistence.FrameworkDbContext", typeof(Framework.Infrastructure.Persistence.FrameworkDbContext).FullName);
        Assert.True(typeof(Framework.Persistence.FrameworkDbContext).IsAssignableFrom(typeof(Framework.Infrastructure.Persistence.FrameworkDbContext)));
    }
}
