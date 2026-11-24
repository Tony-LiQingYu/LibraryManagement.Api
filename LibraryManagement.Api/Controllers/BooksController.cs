using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _dbContext;
    private readonly BookMapper _bookMapper;
    public BooksController(LibraryDbContext dbContext, BookMapper bookMapper)
    {
        _dbContext = dbContext;
        _bookMapper = bookMapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadBookDto>>> GetAll()
    {
        var books = await _dbContext.Books.Include(b => b.Category).Include(b => b.Authors).ToListAsync();
        return Ok(books.Select(b => _bookMapper.ToReadDto(b)).ToList());
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
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == createBookDto.CategoryId);
        if (category is null)
        {
            return Problem(
                detail: $"A category with id {createBookDto.CategoryId} does not exist.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        var authors = await _dbContext.Authors.Where(a => createBookDto.AuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != createBookDto.AuthorIds.Count)
        {
            return Problem(
                detail: "One or more of the provided author ids is not valid.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }   

        var book = _bookMapper.ToEntity(createBookDto);
        book.AvailableCopies = createBookDto.TotalCopies;
        book.Category = category;
        book.Authors = authors;
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var bookDto = _bookMapper.ToReadDto(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, bookDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> Update(int id, UpdateBookDto updateBookDto)
    {
        var existingBook = await _dbContext.Books.FindAsync(id);

        if (existingBook == null)
        {
            return Problem(
                detail: $"A book with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        _bookMapper.UpdateEntity(updateBookDto, existingBook);

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
