namespace Framework.Contracts.Pagination;

public sealed record PageResponse<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PageResponse<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount) =>
        new(items.ToArray(), pageNumber, pageSize, totalCount);
}

