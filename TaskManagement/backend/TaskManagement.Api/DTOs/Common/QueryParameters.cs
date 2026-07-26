namespace TaskManagement.Api.DTOs.Common;

public class QueryParameters
{
    private const int MaxPageSize = 50;

    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    // Search text
    public string? Search { get; set; }

    // name or createdAt
    public string? SortBy { get; set; }

    // asc / desc
    public string? SortDirection { get; set; } = "asc";
}