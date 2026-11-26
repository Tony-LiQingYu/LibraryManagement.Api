using LibraryManagement.Api.RequestParameters.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.RequestParameters;

public class PaginationParameters
{
    private const int MinimalPageNumber = 1;
    public const int DefaultPageNumber = 1;
    private const int MinimalPageSize = 1;
    private const int MaximalPageSize = 50;
    public const int DefaultPageSize = 3;

    private int _pageNumber = DefaultPageNumber;
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get => _pageNumber; set => _pageNumber = value < MinimalPageNumber ? MinimalPageNumber : value; }
    private int _pageSize = DefaultPageSize;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get => _pageSize; set => _pageSize = value < MinimalPageNumber ? MinimalPageNumber : value; }

}

public record SortingParameters<TSortBy> where TSortBy : struct, Enum
{
    [FromQuery(Name = "sortBy")] public TSortBy? SortBy { get; init; }
    [FromQuery(Name = "sortDirection")] public SortDirection SortDirection { get; init; } = SortDirection.Ascending;
}
