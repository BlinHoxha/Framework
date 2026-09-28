using Framework.Contracts.Abstractions;
using Framework.Contracts.Pagination;
using Framework.Domain.Abstractions;

namespace Framework.Application.Abstractions.Services;

public interface IBaseService<TEntity, TEntityId>
    where TEntity : class, IEntity<TEntityId>
{
    Task<TMap?> GetById<TMap>(TEntityId id, CancellationToken cancellationToken = default)
        where TMap : class, IEntityContract<TEntityId>;

    Task<PageResponse<TMap>> Paginate<TMap>(PagedRequest request, CancellationToken cancellationToken = default)
        where TMap : class;

    Task<TEntityId> Add<TMap>(TMap dto, CancellationToken cancellationToken = default)
        where TMap : class;

    Task AddRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default)
        where TMap : class;

    Task<TEntityId> Update<TMap>(TMap dto, CancellationToken cancellationToken = default)
        where TMap : class, IEntityContract<TEntityId>;

    Task UpdateRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default)
        where TMap : class;

    Task Remove(TEntityId id, CancellationToken cancellationToken = default);
    Task RemoveRange(IEnumerable<TEntityId> ids, CancellationToken cancellationToken = default);
}

