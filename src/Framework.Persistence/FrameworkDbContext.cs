using Microsoft.EntityFrameworkCore;

namespace Framework.Persistence;

/// <summary>Provider-neutral EF foundation. Applications own models, configuration and migrations.</summary>
public class FrameworkDbContext : DbContext
{
    public FrameworkDbContext(DbContextOptions<FrameworkDbContext> options) : base(options) { }
    protected FrameworkDbContext(DbContextOptions options) : base(options) { }
}
