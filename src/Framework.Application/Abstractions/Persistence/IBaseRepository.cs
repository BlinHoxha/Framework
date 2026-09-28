using Framework.Contracts.Pagination;
using Framework.Domain.Abstractions;

namespace Framework.Application.Abstractions.Persistence;

public interface IBaseRepository<TEntity, TEntityId>
    where TEntity : class, IEntity<TEntityId>
{
    IQueryable<TEntity> Query(bool track = false);
    Task<TMap?> GetById<TMap>(TEntityId id, CancellationToken cancellationToken = default) where TMap : class;
    Task<TEntity?> GetById(TEntityId id, bool track = false, CancellationToken cancellationToken = default);
    Task<PageResponse<TMap>> Paginate<TMap>(PagedRequest request, CancellationToken cancellationToken = default) where TMap : class;
    Task Add(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task Update(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task Remove(TEntityId id, CancellationToken cancellationToken = default);
    Task RemoveRange(IEnumerable<TEntityId> ids, CancellationToken cancellationToken = default);
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}

