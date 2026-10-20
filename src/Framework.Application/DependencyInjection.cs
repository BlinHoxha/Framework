using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(config => config.AllowNullCollections = true, typeof(Mapping.ApplicationMappingProfile).Assembly);
        return services;
    }
}
