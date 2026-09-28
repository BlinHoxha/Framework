using AutoMapper;
using BuildingBlocks.AI.Core.Abstractions;
using Framework.Application.AI.DocumentProfiles;
using Framework.Application.AI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(config => config.AllowNullCollections = true, typeof(AssemblyMarker).Assembly);
        services.AddSingleton<IDocumentProfileCatalog, DefaultDocumentProfileCatalog>();
        services.AddScoped<DocumentIngestionService>();
        services.AddScoped<KnowledgeAnswerService>();
        return services;
    }
}
