using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoryEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");
        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById).Produces<Category>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound).WithName("GetCategoryById");
        group.MapPost("/", Create).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:int}", Delete).ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<Category>, ProblemHttpResult>> GetById(int id, LibraryDbContext dbContext)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null)
            return TypedResults.Problem(
                detail: $"A category with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound
                );
        return TypedResults.Ok(category);
    }
    private static async Task<Ok<List<Category>>> GetAll(LibraryDbContext dbContext)
    {
        var categories = await dbContext.Categories.ToListAsync();
        return TypedResults.Ok(categories);
    }

    private static async Task<Results<CreatedAtRoute<Category>, ProblemHttpResult>> Create(Category category, LibraryDbContext dbContext)
    {
        if (string.IsNullOrEmpty(category.Name))
            return TypedResults.Problem(
                detail: $"A category with name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest
                );
        if (await dbContext.Categories.AnyAsync(c => c.Id == category.Id))
            return TypedResults.Problem(
                detail: $"A category with Id {category.Id} already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        if (await dbContext.Categories.AnyAsync(c => c.Name == category.Name))
            return TypedResults.Problem(
                detail: $"A category with Name {category.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return TypedResults.CreatedAtRoute(category, "GetCategoryById", new {id = category.Id});
    }

    private static async Task<Results<Ok<Category>, ProblemHttpResult>> Update(int id, Category category, LibraryDbContext dbContext)
    {
        if (string.IsNullOrEmpty(category.Name))
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
        if (await dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == category.Name))
            return TypedResults.Problem(
                detail: $"A category with name {category.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        existingCategory.Name = category.Name;
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(existingCategory);
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
