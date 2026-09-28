using Framework.Application.Abstractions.Persistence;
using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Local;
using BuildingBlocks.AI.Azure;
using Azure.Identity;
using Framework.Infrastructure.AI;
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

        AddAiInfrastructure(services, configuration);

        return services;
    }

    private static void AddAiInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        string providerName = configuration[$"{AiProviderOptions.SectionName}:{nameof(AiProviderOptions.Provider)}"]
            ?? "Local";
        AiProviderOptions options = new() { Provider = providerName };

        services.AddSingleton<ITextChunker, FixedSizeTextChunker>();

        if (string.Equals(options.Provider, "Azure", StringComparison.OrdinalIgnoreCase))
        {
            IConfigurationSection section = configuration.GetSection(AzureAiOptions.SectionName);
            AzureAiOptions azureOptions = AzureAiOptions.FromValues(
                section["OpenAiEndpoint"], section["ChatDeployment"], section["EmbeddingDeployment"],
                section["SearchEndpoint"], section["SearchIndex"]);
            services.AddSingleton(azureOptions);
            services.AddSingleton<AzureTokenClient>(_ => new AzureTokenClient(new HttpClient(), new DefaultAzureCredential()));
            services.AddSingleton<AzureOpenAiClient>();
            services.AddSingleton<IChatModel>(provider => provider.GetRequiredService<AzureOpenAiClient>());
            services.AddSingleton<AzureSearchKnowledgeStore>();
            services.AddSingleton<IKnowledgeIndex>(provider => provider.GetRequiredService<AzureSearchKnowledgeStore>());
            services.AddSingleton<IKnowledgeRetriever>(provider => provider.GetRequiredService<AzureSearchKnowledgeStore>());
            return;
        }

        if (!string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"AI provider '{options.Provider}' is not installed. Add its Infrastructure adapter before selecting it.");
        }

        services.AddSingleton<InMemoryKnowledgeStore>();
        services.AddSingleton<IKnowledgeIndex>(provider => provider.GetRequiredService<InMemoryKnowledgeStore>());
        services.AddSingleton<IKnowledgeRetriever>(provider => provider.GetRequiredService<InMemoryKnowledgeStore>());
        services.AddSingleton<IChatModel, LocalGroundedChatModel>();
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

