using Framework.Contracts.Pagination;

namespace Framework.UnitTests.Pagination;

public sealed class PageResponseTests
{
    [Fact]
    public void Create_Computes_TotalPages_From_TotalCount_And_PageSize()
    {
        PageResponse<int> result = PageResponse<int>.Create([1, 2, 3], 2, 3, 8);

        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(8, result.TotalCount);
    }
}

