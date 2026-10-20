using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Persistence.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameworkSqlServer<TContext>(this IServiceCollection services,
        string connectionString, Action<SqlServerDbContextOptionsBuilder>? configure = null) where TContext : DbContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString, configure));
    }
}
