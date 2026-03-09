namespace PatientHealthRecord.Utilities;

/// <summary>
/// Pagination extension methods for IQueryable
/// </summary>
public static class PaginationExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Max 100 items per page

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
