using System.Linq.Expressions;
using Framework.Contracts.Pagination;

namespace Framework.Infrastructure.Extensions;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, SortDirection direction)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        string[] members = sortBy.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
        Expression body = parameter;

        foreach (string member in members)
        {
            body = Expression.PropertyOrField(body, member);
        }

        LambdaExpression keySelector = Expression.Lambda(body, parameter);
        string methodName = direction == SortDirection.Asc ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

        MethodCallExpression orderedQuery = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), body.Type],
            query.Expression,
            Expression.Quote(keySelector));

        return query.Provider.CreateQuery<T>(orderedQuery);
    }
}

