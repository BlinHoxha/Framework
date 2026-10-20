using Framework.Application.Abstractions.Persistence;
using Framework.Infrastructure.Options;
using Framework.Infrastructure.Persistence;
using Framework.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Framework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        DatabaseOptions databaseOptions = BuildDatabaseOptions(configuration);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(databaseOptions));

        services.AddDbContext<FrameworkDbContext>((serviceProvider, options) =>
        {
            DatabaseOptions optionsSnapshot = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            if (string.Equals(optionsSnapshot.Provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AssemblyMarker).Assembly.FullName);
                    npgsqlOptions.CommandTimeout(optionsSnapshot.CommandTimeoutInSeconds);
                });
            }
            else if (string.Equals(optionsSnapshot.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AssemblyMarker).Assembly.FullName);
                    sqlOptions.CommandTimeout(optionsSnapshot.CommandTimeoutInSeconds);
                    sqlOptions.EnableRetryOnFailure();
                });
            }
            else
            {
                throw new InvalidOperationException($"Database provider '{optionsSnapshot.Provider}' is not supported.");
            }

            if (optionsSnapshot.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

        return services;
    }

    private static DatabaseOptions BuildDatabaseOptions(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(DatabaseOptions.SectionName);

        return new DatabaseOptions
        {
            Provider = section[nameof(DatabaseOptions.Provider)] ?? "PostgreSql",
            CommandTimeoutInSeconds = int.TryParse(section[nameof(DatabaseOptions.CommandTimeoutInSeconds)], out int timeout)
                ? timeout
                : 30,
            EnableSensitiveDataLogging = bool.TryParse(section[nameof(DatabaseOptions.EnableSensitiveDataLogging)], out bool sensitiveLogging)
                && sensitiveLogging
        };
    }
}

