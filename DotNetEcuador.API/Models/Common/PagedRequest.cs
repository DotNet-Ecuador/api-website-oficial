using System.ComponentModel.DataAnnotations;

namespace DotNetEcuador.API.Models.Common;

public class PagedRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page debe ser mayor a 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; } = "asc";

    public string? Search { get; set; }

    public int Skip => (Page - 1) * PageSize;
}