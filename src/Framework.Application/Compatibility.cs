using System.Runtime.CompilerServices;

// Preserve the public names and assembly resolution of existing consuming applications.
[assembly: TypeForwardedTo(typeof(Framework.Application.Abstractions.Persistence.IBaseRepository<,>))]
[assembly: TypeForwardedTo(typeof(Framework.Application.Abstractions.Services.IBaseService<,>))]
[assembly: TypeForwardedTo(typeof(Framework.Application.Services.BaseService<,>))]
[assembly: TypeForwardedTo(typeof(Framework.Application.Mapping.ApplicationMappingProfile))]
[assembly: TypeForwardedTo(typeof(Framework.Application.DependencyInjection))]
