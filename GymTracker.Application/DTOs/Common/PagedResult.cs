namespace GymTracker.Application.DTOs.Common;

/// <summary>Parametri standard de query: ?page=2&pageSize=15&sort=name&search=...</summary>
public class QueryParameters
{
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > 100 => 100, // limitam pt a preveni abuz (DoS via query mare)
            _ => value
        };
    }

    public string? Search { get; set; }
    public string? Sort { get; set; } // ex: "name" sau "-name" pt descrescator
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
