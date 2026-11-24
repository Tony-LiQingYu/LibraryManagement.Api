using LibraryManagement.Api.DTOs.Books;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Authors;

public record CreateAuthorDto
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; init; }
    [Required]
    public required string LastName { get; init; }
    [Range(-2000, 2026)]
    public int? YearOfBirth { get; init; }
}

public record UpdateAuthorDto
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; init; }
    [Required]
    public required string LastName { get; init; }
    [Range(-2000, 2026)]
    public int? YearOfBirth { get; init; }
}

public record ReadAuthorDto
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string FamilyName { get; init; }
    public int? YearOfBirth { get; init; }
    public ICollection<AuthorBookDto> Books { get; init; } = [];
}
