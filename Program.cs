using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// OpenAPI/Swagger helps explore endpoints and payloads during development.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Publish the OpenAPI document and interactive Swagger UI for learning/testing.
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Todo API v1");
    });
}

app.UseHttpsRedirection();

// Simple root endpoint to verify the API is running.
app.MapGet("/", () => Results.Ok(new { message = "Todo API is running" }));

app.Run();
