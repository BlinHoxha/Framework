using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Framework.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FrameworkDbContext>
{
    public FrameworkDbContext CreateDbContext(string[] args)
    {
        const string fallbackConnectionString = "Host=localhost;Port=5432;Database=framework;Username=postgres;Password=postgres";

        DbContextOptionsBuilder<FrameworkDbContext> optionsBuilder = new();
        string provider = Environment.GetEnvironmentVariable("FRAMEWORK_DATABASE_PROVIDER") ?? "PostgreSql";
        string connectionString = Environment.GetEnvironmentVariable("FRAMEWORK_CONNECTION_STRING") ?? fallbackConnectionString;

        if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AssemblyMarker).Assembly.FullName));
        }
        else if (string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AssemblyMarker).Assembly.FullName));
        }
        else
        {
            throw new InvalidOperationException($"Database provider '{provider}' is not supported.");
        }

        return new FrameworkDbContext(optionsBuilder.Options);
    }
}

