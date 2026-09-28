using Microsoft.EntityFrameworkCore;

namespace Framework.Infrastructure.Persistence;

public sealed class FrameworkDbContext(DbContextOptions<FrameworkDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
