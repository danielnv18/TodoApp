using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TodoApi.Contracts;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// OpenAPI/Swagger helps explore endpoints and payloads during development.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHealthChecks().AddDbContextCheck<TodoDbContext>();
builder.Services.AddProblemDetails();

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

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        var problem = Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            detail: exception?.Error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = context.TraceIdentifier
            });

        await problem.ExecuteAsync(context);
    });
});

app.UseHttpsRedirection();

// Simple root endpoint to verify the API is running.
app.MapGet("/", () => Results.Ok(new { message = "Todo API is running" }));
app.MapHealthChecks("/health");

var todos = app.MapGroup("/todos").WithTags("Todos");

todos.MapGet("/", async (TodoDbContext db) =>
{
    var items = await db.Todos.AsNoTracking()
        .OrderBy(t => t.Id)
        .Select(t => new TodoResponse(t.Id, t.Name, t.IsComplete, t.CreatedAtUtc))
        .ToListAsync();
    return Results.Ok(items);
});

todos.MapGet("/{id:int}", async (int id, TodoDbContext db) =>
{
    var todo = await db.Todos.AsNoTracking()
        .Where(t => t.Id == id)
        .Select(t => new TodoResponse(t.Id, t.Name, t.IsComplete, t.CreatedAtUtc))
        .FirstOrDefaultAsync();

    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

todos.MapPost("/", async (TodoCreateRequest request, TodoDbContext db) =>
{
    var validationErrors = ValidationHelpers.Validate(request);
    if (validationErrors is not null)
    {
        return Results.ValidationProblem(validationErrors);
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Name"] = ["Name cannot be empty or whitespace."]
        });
    }

    var todo = new Todo
    {
        Name = request.Name.Trim(),
        IsComplete = request.IsComplete,
        CreatedAtUtc = DateTime.UtcNow
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    var response = new TodoResponse(todo.Id, todo.Name, todo.IsComplete, todo.CreatedAtUtc);
    return Results.Created($"/todos/{todo.Id}", response);
});

todos.MapPut("/{id:int}", async (int id, TodoUpdateRequest request, TodoDbContext db) =>
{
    var validationErrors = ValidationHelpers.Validate(request);
    if (validationErrors is not null)
    {
        return Results.ValidationProblem(validationErrors);
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Name"] = ["Name cannot be empty or whitespace."]
        });
    }

    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todo.Name = request.Name.Trim();
    todo.IsComplete = request.IsComplete;
    await db.SaveChangesAsync();

    return Results.Ok(new TodoResponse(todo.Id, todo.Name, todo.IsComplete, todo.CreatedAtUtc));
});

todos.MapPatch("/{id:int}/status", async (int id, TodoCompletionRequest request, TodoDbContext db) =>
{
    var validationErrors = ValidationHelpers.Validate(request);
    if (validationErrors is not null)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todo.IsComplete = request.IsComplete;
    await db.SaveChangesAsync();

    return Results.Ok(new TodoResponse(todo.Id, todo.Name, todo.IsComplete, todo.CreatedAtUtc));
});

todos.MapDelete("/{id:int}", async (int id, TodoDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
