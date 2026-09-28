using Framework.Application.Abstractions.Services;
using Framework.Contracts.Abstractions;
using Framework.Domain.Abstractions;

namespace Framework.Api.Controllers;

// Compatibility facade. New consumers reference Framework.Web directly.
public abstract class BaseControllerCRUD<TDto, TDtoGrid, TDtoAdd, TDtoEdit, TEntity, TEntityId, TService>(
    ILogger logger, TService entityService)
    : Framework.Web.Controllers.BaseControllerCRUD<TDto, TDtoGrid, TDtoAdd, TDtoEdit, TEntity, TEntityId, TService>(logger, entityService)
    where TDto : class, IEntityContract<TEntityId>
    where TDtoGrid : class
    where TDtoAdd : class
    where TDtoEdit : class, IEntityContract<TEntityId>
    where TEntity : class, IEntity<TEntityId>
    where TService : IBaseService<TEntity, TEntityId>;
