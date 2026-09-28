using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Framework.Application.Abstractions.Services;
using Framework.Contracts.Abstractions;
using Framework.Contracts.Common;
using Framework.Contracts.Pagination;
using Framework.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Web.Controllers;

[ApiController]
public abstract class BaseControllerRO<TDto, TDtoGrid, TEntity, TEntityId, TService>(
    ILogger logger,
    TService entityService) : ControllerBase
    where TDto : class, IEntityContract<TEntityId>
    where TDtoGrid : class
    where TEntity : class, IEntity<TEntityId>
    where TService : IBaseService<TEntity, TEntityId>
{
    protected ILogger Logger { get; } = logger;
    protected TService EntityService { get; } = entityService;

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<CommonResult<TDto>>> GetById([Required] TEntityId id, CancellationToken cancellationToken)
    {
        TDto? result = await EntityService.GetById<TDto>(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(CommonResult<TDto>.Success(result));
    }

    [HttpGet("filtered-search")]
    public virtual async Task<ActionResult<CommonResult<PageResponse<TDtoGrid>>>> Search([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        PageResponse<TDtoGrid> result = await EntityService.Paginate<TDtoGrid>(request, cancellationToken);
        return Ok(CommonResult<PageResponse<TDtoGrid>>.Success(result));
    }
}


