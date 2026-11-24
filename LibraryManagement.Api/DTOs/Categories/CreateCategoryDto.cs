namespace LibraryManagement.Api.DTOs.Categories;

public record CreateCategoryDto
{
    public required string Name { get; init; }
}

public record UpdateCategoryDto
{
    public required string Name { get; init; }
}

public record ReadCategoryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}