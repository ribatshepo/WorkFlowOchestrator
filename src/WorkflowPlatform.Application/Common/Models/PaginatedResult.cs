namespace WorkflowPlatform.Application.Common.Models;

/// <summary>
/// Represents a paginated result with metadata
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Creates an empty paginated result
    /// </summary>
    /// <returns>An empty paginated result</returns>
    public static PaginatedResult<T> Empty()
    {
        return new PaginatedResult<T>
        {
            Items = Array.Empty<T>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0
        };
    }

    /// <summary>
    /// Creates a paginated result from the given parameters
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="totalCount">The total number of items</param>
    /// <param name="page">The current page number</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>A paginated result</returns>
    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}
