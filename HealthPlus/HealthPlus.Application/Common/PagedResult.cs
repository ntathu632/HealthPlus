namespace HealthPlus.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPrevPage => Page > 1;

    public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var list = source.ToList();
        return new PagedResult<T>
        {
            TotalCount = list.Count,
            Page = page,
            PageSize = pageSize,
            Items = list.Skip((page - 1) * pageSize).Take(pageSize)
        };
    }
}
