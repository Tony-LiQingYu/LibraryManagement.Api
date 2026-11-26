using ErrorOr;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Entities;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.RequestParameters.Enums;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : WebApiController
{
    private readonly LibraryDbContext _dbContext;
    private readonly BookMapper _bookMapper;
    private readonly IBooksService _booksService;
    public BooksController(LibraryDbContext dbContext, BookMapper bookMapper, IBooksService booksService)
    {
        _dbContext = dbContext;
        _bookMapper = bookMapper;
        _booksService = booksService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadBookDto>>> GetAll([FromQuery] PaginationParameters pagination,
        [FromQuery] BookFilteringParameters filteringParameters,
        [FromQuery] SortingParameters<BookSortBy> sortingParameters)
    { 
        var result = await _booksService.GetAllAsync(pagination, filteringParameters, sortingParameters);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }
        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.Value.TotalPages.ToString());
        return Ok(result.Value.Items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> GetById(int id)
    {
        var book = await _dbContext.Books.Include(b => b.Category).Include(b => b.Authors).FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
        {
            return Problem(
                detail: $"A book with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }
        var bookDto = _bookMapper.ToReadDto(book);
        return Ok(bookDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadBookDto>> Create(CreateBookDto createBookDto)
    {
        var result = await _booksService.CreateAsync(createBookDto);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> Update(int id, UpdateBookDto updateBookDto)
    {
        var existingBook = await _dbContext.Books.Include(b => b.Authors).FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook == null)
        {
            return Problem(
                detail: $"A book with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == updateBookDto.CategoryId);
        if (category is null)
        {
            return Problem(
                detail: $"A category with id {updateBookDto.CategoryId} does not exist.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        var uniqueAuthorIds = updateBookDto.AuthorIds.Distinct().ToList();
        var authors = await _dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return Problem(
                detail: "One or more of the provided author ids is not valid.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var oldTotalCopies = existingBook.TotalCopies;
        var newTotalCopies = updateBookDto.TotalCopies;
        var totalCopiesAdded = newTotalCopies - oldTotalCopies;
        if (totalCopiesAdded > 0)
        {
            existingBook.AvailableCopies += totalCopiesAdded;
        }
        else if(totalCopiesAdded < 0)
        {
            existingBook.AvailableCopies += totalCopiesAdded;
        }

        _bookMapper.UpdateEntity(updateBookDto, existingBook);
        existingBook.Category = category;
        existingBook.Authors = authors;

        await _dbContext.SaveChangesAsync();

        return Ok(_bookMapper.ToReadDto(existingBook));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var book = await _dbContext.Books.FindAsync(id);

        if (book == null)
        {
            return Problem(
                detail: $"A book with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
