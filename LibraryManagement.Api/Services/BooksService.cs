using ErrorOr;
using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Errors;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.RequestParameters.Enums;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class BooksService : IBooksService
{
    private readonly LibraryDbContext _dbContext;
    private readonly BookMapper _bookMapper;
    public BooksService(LibraryDbContext dbContext, BookMapper bookMapper)
    {
        _dbContext = dbContext;
        _bookMapper = bookMapper;
    }
    public async Task<ErrorOr<ReadBookDto>> CreateAsync(CreateBookDto createDto)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == createDto.CategoryId);
        if (category is null)
        {
            return BookErrors.CategoryNotFound(createDto.CategoryId);
        }
        var uniqueAuthorIds = createDto.AuthorIds.Distinct().ToList();

        var authors = await _dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return BookErrors.AuthorNotFound();
        }

        var book = _bookMapper.ToEntity(createDto);
        book.AvailableCopies = createDto.TotalCopies;
        book.Category = category;
        book.Authors = authors;
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        return _bookMapper.ToReadDto(book);
    }

    public async Task<ErrorOr<PagedResult<ReadBookDto>>> GetAllAsync(PaginationParameters pagination,
       BookFilteringParameters filteringParameters,
       SortingParameters<BookSortBy> sortingParameters)
    {
        var query = _dbContext.Books.Include(b => b.Category).Include(b => b.Authors).AsQueryable();
        if (filteringParameters.Title is not null)
        {
            query = query.Where(b => b.Title.Contains(filteringParameters.Title));
        }

        if (filteringParameters.CategoryId is not null)
        {
            query = query.Where(b => b.CategoryId == filteringParameters.CategoryId);
        }

        if (filteringParameters.AuthorId is not null)
        {
            query = query.Where(b => b.Authors.Any(a => a.Id == filteringParameters.AuthorId));
        }

        query = sortingParameters.SortBy switch
        {
            BookSortBy.Title => sortingParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(b => b.Title)
                : query.OrderByDescending(b => b.Title),
            BookSortBy.AvailableCopies => sortingParameters.SortDirection == SortDirection.Ascending
            ? query.OrderBy(b => b.AvailableCopies)
            : query.OrderByDescending(b => b.AvailableCopies),
            _ => query.OrderBy(b => b.Id)
        };
        var totalCount = await query.CountAsync();
        
        var books = await query.Skip(pagination.PageSize * (pagination.PageNumber - 1)).Take(pagination.PageSize).ToListAsync();
        var readDtos = books.Select(b => _bookMapper.ToReadDto(b)).ToList();
        
        return new PagedResult<ReadBookDto>(readDtos, totalCount, pagination);
    }
}
