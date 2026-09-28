using AutoMapper;
using Framework.Application.Abstractions.Persistence;
using Framework.Application.Abstractions.Services;
using Framework.Contracts.Abstractions;
using Framework.Contracts.Pagination;
using Framework.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Framework.Application.Services;

public abstract class BaseService<TEntity, TEntityId>(
    IBaseRepository<TEntity, TEntityId> repository,
    ILogger logger,
    IMapper mapper) : IBaseService<TEntity, TEntityId>
    where TEntity : class, IEntity<TEntityId>
{
    protected IBaseRepository<TEntity, TEntityId> Repository { get; } = repository;
    protected ILogger Logger { get; } = logger;
    protected IMapper Mapper { get; } = mapper;

    public virtual Task<TMap?> GetById<TMap>(TEntityId id, CancellationToken cancellationToken = default)
        where TMap : class, IEntityContract<TEntityId>
    {
        ArgumentNullException.ThrowIfNull(id);
        return Repository.GetById<TMap>(id, cancellationToken);
    }

    public virtual Task<PageResponse<TMap>> Paginate<TMap>(PagedRequest request, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(request);
        return Repository.Paginate<TMap>(request, cancellationToken);
    }

    public virtual async Task<TEntityId> Add<TMap>(TMap dto, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(dto);

        TEntity entity = Mapper.Map<TEntity>(dto);
        await Repository.Add(entity, cancellationToken);
        await Repository.SaveChanges(cancellationToken);

        Logger.LogTrace("Added {EntityType} with id {EntityId}", typeof(TEntity).Name, entity.Id);
        return entity.Id;
    }

    public virtual async Task AddRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(dtos);

        IEnumerable<TEntity> entities = Mapper.Map<IEnumerable<TEntity>>(dtos);
        await Repository.AddRange(entities, cancellationToken);
        await Repository.SaveChanges(cancellationToken);
    }

    public virtual async Task<TEntityId> Update<TMap>(TMap dto, CancellationToken cancellationToken = default)
        where TMap : class, IEntityContract<TEntityId>
    {
        ArgumentNullException.ThrowIfNull(dto);

        TEntity existingEntity = await Repository.GetById(dto.Id, true, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{dto.Id}' was not found.");

        Mapper.Map(dto, existingEntity);
        await Repository.Update(existingEntity, cancellationToken);
        await Repository.SaveChanges(cancellationToken);

        Logger.LogTrace("Updated {EntityType} with id {EntityId}", typeof(TEntity).Name, existingEntity.Id);
        return existingEntity.Id;
    }

    public virtual async Task UpdateRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(dtos);

        IEnumerable<TEntity> entities = Mapper.Map<IEnumerable<TEntity>>(dtos);
        await Repository.UpdateRange(entities, cancellationToken);
        await Repository.SaveChanges(cancellationToken);
    }

    public virtual async Task Remove(TEntityId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        await Repository.Remove(id, cancellationToken);
        await Repository.SaveChanges(cancellationToken);

        Logger.LogTrace("Removed {EntityType} with id {EntityId}", typeof(TEntity).Name, id);
    }

    public virtual async Task RemoveRange(IEnumerable<TEntityId> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        await Repository.RemoveRange(ids, cancellationToken);
        await Repository.SaveChanges(cancellationToken);
    }
}

