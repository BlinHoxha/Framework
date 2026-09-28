using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Application.Abstractions.Persistence;
using Framework.Contracts.Pagination;
using Framework.Domain.Abstractions;
using Framework.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Framework.Infrastructure.Persistence.Repositories;

public class BaseRepository<TEntity, TEntityId, TContext>(
    TContext context,
    ILogger logger,
    IMapper mapper) : IBaseRepository<TEntity, TEntityId>
    where TEntity : class, IEntity<TEntityId>
    where TContext : DbContext
{
    protected TContext Context { get; } = context;
    protected ILogger Logger { get; } = logger;
    protected IMapper Mapper { get; } = mapper;
    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();

    public virtual IQueryable<TEntity> Query(bool track = false) =>
        track ? DbSet : DbSet.AsNoTracking();

    public virtual Task<TMap?> GetById<TMap>(TEntityId id, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(id);

        return Query()
            .Where(entity => entity.Id!.Equals(id))
            .ProjectTo<TMap>(Mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual Task<TEntity?> GetById(TEntityId id, bool track = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return Query(track)
            .FirstOrDefaultAsync(entity => entity.Id!.Equals(id), cancellationToken);
    }

    public virtual async Task<PageResponse<TMap>> Paginate<TMap>(PagedRequest request, CancellationToken cancellationToken = default)
        where TMap : class
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<TMap> projectedQuery = Query()
            .ProjectTo<TMap>(Mapper.ConfigurationProvider)
            .ApplySorting(request.SortBy, request.SortDirection);

        int totalCount = await projectedQuery.CountAsync(cancellationToken);
        List<TMap> items = await projectedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PageResponse<TMap>.Create(items, request.PageNumber, request.PageSize, totalCount);
    }

    public virtual Task Add(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return DbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public virtual Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task Remove(TEntityId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        TEntity entity = await GetById(id, true, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");

        DbSet.Remove(entity);
    }

    public virtual async Task RemoveRange(IEnumerable<TEntityId> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        TEntityId[] idArray = ids.ToArray();
        List<TEntity> entities = await Query(true)
            .Where(entity => idArray.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(entities);
    }

    public virtual Task<int> SaveChanges(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}


// Compatibility registration for the Framework reference host.
public class BaseRepository<TEntity, TEntityId>(
    FrameworkDbContext context,
    ILogger<BaseRepository<TEntity, TEntityId>> logger,
    IMapper mapper) : BaseRepository<TEntity, TEntityId, FrameworkDbContext>(context, logger, mapper)
    where TEntity : class, IEntity<TEntityId>;
