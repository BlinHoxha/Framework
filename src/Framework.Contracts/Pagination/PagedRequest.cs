using System.ComponentModel.DataAnnotations;

namespace Framework.Contracts.Pagination;

public sealed record PagedRequest
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = 1;

    [Range(1, 500)]
    public int PageSize { get; init; } = 20;

    [MaxLength(128)]
    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}

