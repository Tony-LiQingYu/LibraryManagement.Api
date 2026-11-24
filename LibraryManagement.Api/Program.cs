using FluentValidation;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.Endpoints;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Middleware;
using LibraryManagement.Api.Validators.Categories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("LibraryDb"));
});
builder.Services.AddScoped<AuthorMapper>();
builder.Services.AddScoped<CategoryMapper>();
builder.Services.AddScoped<BookMapper>();
builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();
//builder.Services.AddScoped<IValidator<CreateBookDto>, CreateBookDtoValidator>();
//builder.Services.AddScoped<IValidator<UpdateBookDto>, UpdateBookDtoValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<LogActionFilter>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<RequestTimingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await DataSeeder.SeedAsync(app.Services);
}

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");
app.MapCategoryEndPoints();
app.MapControllers();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
