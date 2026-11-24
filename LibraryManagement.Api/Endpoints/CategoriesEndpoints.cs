using FluentValidation;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.Entities;
using LibraryManagement.Api.Extensions;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoryEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");
        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById).Produces<Category>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound).WithName("GetCategoryById");
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateCategoryDto>>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update).AddEndpointFilter<ValidationFilter<UpdateCategoryDto>>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:int}", Delete).ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ReadCategoryDto>, ProblemHttpResult>> GetById(int id, LibraryDbContext dbContext, CategoryMapper mapper)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null)
            return TypedResults.Problem(
                detail: $"A category with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
                );
        var readDto = mapper.ToReadDto(category);
        return TypedResults.Ok(readDto);
    }

    private static async Task<Ok<List<ReadCategoryDto>>> GetAll(LibraryDbContext dbContext, CategoryMapper mapper)
    {
        var categories = await dbContext.Categories.ToListAsync();
        var readDtos = categories.Select(mapper.ToReadDto).ToList();
        return TypedResults.Ok(readDtos);
    }

    private static async Task<Results<CreatedAtRoute<ReadCategoryDto>, ProblemHttpResult>> Create(CreateCategoryDto categoryDto, LibraryDbContext dbContext, CategoryMapper mapper)
    { 
        if (await dbContext.Categories.AnyAsync(c => c.Name == categoryDto.Name))
            return TypedResults.Problem(
                detail: $"A category with Name {categoryDto.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        var category = mapper.ToEntity(categoryDto);
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        var readDto = mapper.ToReadDto(category);
        return TypedResults.CreatedAtRoute(readDto, "GetCategoryById", new { id = category.Id });
    }

    private static async Task<Results<Ok<ReadCategoryDto>, ProblemHttpResult>> Update(int id, UpdateCategoryDto categoryDto, LibraryDbContext dbContext, CategoryMapper mapper)
    {
        if (string.IsNullOrEmpty(categoryDto.Name))
            return TypedResults.Problem(
                detail: "Category name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest
                );
        var existingCategory = await dbContext.Categories.FindAsync(id);
        if (existingCategory is null)
            return TypedResults.Problem(
                detail: $"A category with Id {id} cannot be found.",
                statusCode: StatusCodes.Status404NotFound
                );
        if (await dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == categoryDto.Name))
            return TypedResults.Problem(
                detail: $"A category with name {categoryDto.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        mapper.UpdateEntity(categoryDto, existingCategory);
        await dbContext.SaveChangesAsync();
        var readDto = mapper.ToReadDto(existingCategory);
        return TypedResults.Ok(readDto);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(int id, LibraryDbContext dbContext)
    {
        var categorytoBeDeleted = await dbContext.Categories.FindAsync(id);

        if (categorytoBeDeleted is null)
            return TypedResults.Problem(
               detail: $"A category with Id {id} has not been found.",
               statusCode: StatusCodes.Status404NotFound
               );

        dbContext.Categories.Remove(categorytoBeDeleted);
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
