using Framework.Application.Abstractions.Services;
using Framework.Contracts.Abstractions;
using Framework.Domain.Abstractions;

namespace Framework.Api.Controllers;

// Compatibility facade. New consumers reference Framework.Web directly.
public abstract class BaseControllerRO<TDto, TDtoGrid, TEntity, TEntityId, TService>(
    ILogger logger, TService entityService)
    : Framework.Web.Controllers.BaseControllerRO<TDto, TDtoGrid, TEntity, TEntityId, TService>(logger, entityService)
    where TDto : class, IEntityContract<TEntityId>
    where TDtoGrid : class
    where TEntity : class, IEntity<TEntityId>
    where TService : IBaseService<TEntity, TEntityId>;
