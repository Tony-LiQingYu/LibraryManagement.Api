using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.Entities;
using Riok.Mapperly.Abstractions;

namespace LibraryManagement.Api.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AuthorMapper
{
    // Fix RMG023 and RMG020: Map LastName to FamilyName and ignore LastName on target
    [MapProperty(nameof(Author.LastName), nameof(ReadAuthorDto.FamilyName))]
    public partial ReadAuthorDto ToReadDto(Entities.Author source);

    [MapProperty(nameof(Author.LastName), nameof(ReadAuthorDetailDto.FamilyName))]
    public partial ReadAuthorDetailDto ToReadDetailDto(Entities.Author source);
    [MapperIgnoreTarget(nameof(Author.Id))]
    [MapperIgnoreTarget(nameof(Author.Books))]
    public partial Author ToEntity(CreateAuthorDto source);
    [MapperIgnoreTarget(nameof(Author.Id))]
    [MapperIgnoreTarget(nameof(Author.Books))]
    public partial void UpdateEntity(UpdateAuthorDto source, Author target);
}

[Mapper]
public partial class CategoryMapper
{
    public partial ReadCategoryDto ToReadDto(Entities.Category source);
    [MapperIgnoreTarget(nameof(Category.Id))]
    public partial Category ToEntity(CreateCategoryDto source);
    [MapperIgnoreTarget(nameof(Category.Id))]
    public partial void UpdateEntity(UpdateCategoryDto source, Category target);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookMapper
{
    public partial ReadBookDto ToReadDto(Entities.Book source);

    [MapperIgnoreTarget(nameof(Book.Id))]
    [MapperIgnoreTarget(nameof(Book.AvailableCopies))]
    [MapperIgnoreTarget(nameof(Book.Category))]
    [MapperIgnoreTarget(nameof(Book.Authors))]
    public partial Book ToEntity(CreateBookDto source);
    [MapperIgnoreTarget(nameof(Book.Id))]
    [MapperIgnoreTarget(nameof(Book.AvailableCopies))]
    [MapperIgnoreTarget(nameof(Book.Category))]
    [MapperIgnoreTarget(nameof(Book.Authors))]
    public partial void UpdateEntity(UpdateBookDto source, Book target);
}