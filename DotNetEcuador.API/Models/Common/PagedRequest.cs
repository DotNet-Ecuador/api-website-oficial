namespace DotNetEcuador.API.Models.Common;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public string? Search { get; set; }

    public int Skip => (Page - 1) * PageSize;
}