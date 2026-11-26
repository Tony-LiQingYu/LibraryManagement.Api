using LibraryManagement.Api.DTOs.Authors;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Books;

public class ReadBookDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required int AvailableCopies { get; init; }
    public required int TotalCopies { get; init; }
    public required int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public ICollection<BookAuthorDto> Authors { get; init; } = [];
}

public class CreateBookDto
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int TotalCopies { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int CategoryId { get; init; }

    [MinLength(1)]
    public required ICollection<int> AuthorIds { get; init; }
}
public class UpdateBookDto
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int TotalCopies { get; init; }


    [Required]
    [Range(1, int.MaxValue)]
    public required int CategoryId { get; init; }

    [MinLength(1)]
    public required ICollection<int> AuthorIds { get; init; }
}