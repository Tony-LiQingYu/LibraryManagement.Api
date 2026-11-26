using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Entities;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.RequestParameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly LibraryDbContext _dbContext;
    private readonly AuthorMapper _authorMapper;

    public AuthorsController(LibraryDbContext dbContext, AuthorMapper authorMapper)
    {
        _dbContext = dbContext;
        _authorMapper = authorMapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDetailDto>>> GetAll([FromQuery] PaginationParameters pagination)
    {
        var authors = await _dbContext.Authors.OrderBy(a => a.Id).Skip(pagination.PageSize * (pagination.PageNumber - 1)).Take(pagination.PageSize).ToListAsync();
        return Ok(authors.Select(a => _authorMapper.ToReadDetailDto(a)).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDto>> GetById(int id)
    {
        var author = await _dbContext.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null)
        {
            return Problem(
                detail: $"An author with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }
        var authorDto = _authorMapper.ToReadDto(author);
        return Ok(authorDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDto>> Create(CreateAuthorDto createAuthorDto)
    {
        var author = _authorMapper.ToEntity(createAuthorDto);
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        var authorDto = _authorMapper.ToReadDto(author);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, authorDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDto>> Update(int id, UpdateAuthorDto updateAuthorDto)
    {
        var existingAuthor = await _dbContext.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);

        if (existingAuthor == null)
        {
            return Problem(
                detail: $"An author with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        _authorMapper.UpdateEntity(updateAuthorDto, existingAuthor);

        await _dbContext.SaveChangesAsync();

        return Ok(_authorMapper.ToReadDto(existingAuthor));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        var author = await _dbContext.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return Problem(
                detail: $"An author with id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        if (author.Books.Any())
        {
            return Problem(
                detail: $"Cannot delete author with id {id}, because they have associated books. Please remove or reassign the books first.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
