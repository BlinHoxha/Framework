using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Framework.Persistence.PostgreSql;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameworkPostgreSql<TContext>(this IServiceCollection services,
        string connectionString, Action<NpgsqlDbContextOptionsBuilder>? configure = null) where TContext : DbContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString, configure));
    }
}
