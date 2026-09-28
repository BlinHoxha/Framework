using Microsoft.EntityFrameworkCore;

namespace Framework.Infrastructure.Persistence;

public class FrameworkDbContext : DbContext
{
    public FrameworkDbContext(DbContextOptions<FrameworkDbContext> options) : base(options)
    {
    }

    // Derived contexts supply their own typed options through this constructor.
    protected FrameworkDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
