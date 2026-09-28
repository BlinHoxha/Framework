using System.ComponentModel.DataAnnotations;
using Framework.Application.Abstractions.Services;
using Framework.Contracts.Abstractions;
using Framework.Contracts.Common;
using Framework.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Api.Controllers;

[ApiController]
public abstract class BaseControllerCRUD<TDto, TDtoGrid, TDtoAdd, TDtoEdit, TEntity, TEntityId, TService>(
    ILogger logger,
    TService entityService) : BaseControllerRO<TDto, TDtoGrid, TEntity, TEntityId, TService>(logger, entityService)
    where TDto : class, IEntityContract<TEntityId>
    where TDtoGrid : class
    where TDtoAdd : class
    where TDtoEdit : class, IEntityContract<TEntityId>
    where TEntity : class, IEntity<TEntityId>
    where TService : IBaseService<TEntity, TEntityId>
{
    [HttpPost]
    public virtual async Task<ActionResult<CommonResult<EntityIdResponse<TEntityId>>>> Add([FromBody] TDtoAdd item, CancellationToken cancellationToken)
    {
        TEntityId createdId = await EntityService.Add(item, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, CommonResult<EntityIdResponse<TEntityId>>.Success(new EntityIdResponse<TEntityId>(createdId)));
    }

    [HttpPut]
    public virtual async Task<ActionResult<CommonResult<EntityIdResponse<TEntityId>>>> Update([FromBody] TDtoEdit item, CancellationToken cancellationToken)
    {
        TEntityId updatedId = await EntityService.Update(item, cancellationToken);
        return Ok(CommonResult<EntityIdResponse<TEntityId>>.Success(new EntityIdResponse<TEntityId>(updatedId)));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(CommonResult<object>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<CommonResult<object>>> Delete([Required] TEntityId id, CancellationToken cancellationToken)
    {
        await EntityService.Remove(id, cancellationToken);
        return Ok(CommonResult<object>.Success(null));
    }
}

