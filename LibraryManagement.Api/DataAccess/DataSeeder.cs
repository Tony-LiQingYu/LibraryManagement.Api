using LibraryManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.DataAccess;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        if (await dbContext.Authors.AnyAsync())
        {
            return; // Database has been seeded
        }

        var authors = new List<Author>
        {
            new Author { FirstName = "John", LastName = "Doe", YearOfBirth = 1980 },
            new Author { FirstName = "Jane", LastName = "Smith", YearOfBirth = 1990 },
            new Author { FirstName = "Bob", LastName = "Johnson", YearOfBirth = 1975 },
            new Author { FirstName = "Alice", LastName = "Williams", YearOfBirth = 1985 },
            new Author { FirstName = "Charlie", LastName = "Brown", YearOfBirth = 1992 }
        };

        await dbContext.Authors.AddRangeAsync(authors);
        await dbContext.SaveChangesAsync();

        var categories = new List<Category>
        {
            new Category { Name = "Fiction" },
            new Category { Name = "Non-Fiction" },
            new Category { Name = "Science Fiction" },
            new Category { Name = "Fantasy" },
            new Category { Name = "Mystery" }
        };

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
    }
}
