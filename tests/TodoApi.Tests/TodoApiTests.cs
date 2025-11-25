using System.Net;
using System.Net.Http.Json;
using Xunit;
using TodoApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace TodoApi.Tests;

public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TodoApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));
                
                // Add in-memory database for testing
                services.AddDbContext<TodoDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });
                
                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    #region Root Endpoint Tests

    [Fact]
    public async Task Get_Root_ReturnsOkWithMessage()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(content);
    }

    #endregion

    #region GET /todos Tests

    [Fact]
    public async Task GetTodos_WhenEmpty_ReturnsEmptyList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/todos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetTodos_WithExistingTodos_ReturnsAllTodos()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Task 1", false));
        await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Task 2", true));
        var response = await client.GetAsync("/todos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        Assert.NotNull(todos);
        Assert.Equal(2, todos.Count);
        Assert.Equal("Task 1", todos[0].Name);
        Assert.Equal("Task 2", todos[1].Name);
    }

    [Fact]
    public async Task GetTodos_ReturnsOrderedById()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/todos", new TodoCreateRequest("First", false));
        await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Second", false));
        var response = await client.GetAsync("/todos");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        Assert.NotNull(todos);
        Assert.True(todos[0].Id < todos[1].Id);
    }

    #endregion

    #region GET /todos/{id} Tests

    [Fact]
    public async Task GetTodoById_WhenExists_ReturnsOkWithTodo()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Test Todo", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var response = await client.GetAsync($"/todos/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(todo);
        Assert.Equal(created.Id, todo.Id);
        Assert.Equal("Test Todo", todo.Name);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task GetTodoById_WhenNotExists_ReturnsNotFound()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/todos/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /todos Tests

    [Fact]
    public async Task CreateTodo_WithValidData_ReturnsCreatedWithLocation()
    {
        var client = CreateClient();
        var request = new TodoCreateRequest("New Task", false);
        var response = await client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(todo);
        Assert.Equal("New Task", todo.Name);
        Assert.False(todo.IsComplete);
        Assert.True((DateTime.UtcNow - todo.CreatedAtUtc) < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateTodo_WithIsCompleteTrue_CreatesCompletedTodo()
    {
        var client = CreateClient();
        var request = new TodoCreateRequest("Completed Task", true);
        var response = await client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.True(todo.IsComplete);
    }

    [Fact]
    public async Task CreateTodo_WithWhitespaceName_TrimmsName()
    {
        var client = CreateClient();
        var request = new TodoCreateRequest("  Trimmed Task  ", false);
        var response = await client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("Trimmed Task", todo.Name);
    }

    [Fact]
    public async Task CreateTodo_WithEmptyName_ReturnsBadRequest()
    {
        var client = CreateClient();
        var request = new TodoCreateRequest("", false);
        var response = await client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(error);
    }

    [Fact]
    public async Task CreateTodo_WithWhitespaceOnlyName_ReturnsBadRequest()
    {
        var client = CreateClient();
        var request = new TodoCreateRequest("   ", false);
        var response = await client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_WithNullName_ReturnsBadRequest()
    {
        var client = CreateClient();
        var content = JsonContent.Create(new { name = (string?)null, isComplete = false });
        var response = await client.PostAsync("/todos", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region PUT /todos/{id} Tests

    [Fact]
    public async Task UpdateTodo_WhenExists_ReturnsOkWithUpdatedTodo()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Original", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var updateRequest = new TodoUpdateRequest("Updated Name", true);
        var response = await client.PutAsJsonAsync($"/todos/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("Updated Name", updated.Name);
        Assert.True(updated.IsComplete);
        Assert.Equal(created.CreatedAtUtc, updated.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateTodo_WhenNotExists_ReturnsNotFound()
    {
        var client = CreateClient();
        var updateRequest = new TodoUpdateRequest("Updated", false);
        var response = await client.PutAsJsonAsync("/todos/999", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_WithEmptyName_ReturnsBadRequest()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Original", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var updateRequest = new TodoUpdateRequest("", false);
        var response = await client.PutAsJsonAsync($"/todos/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_WithWhitespaceName_TrimmsAndUpdates()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Original", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var updateRequest = new TodoUpdateRequest("  Trimmed Update  ", false);
        var response = await client.PutAsJsonAsync($"/todos/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("Trimmed Update", updated.Name);
    }

    #endregion

    #region PATCH /todos/{id}/status Tests

    [Fact]
    public async Task PatchTodoStatus_WhenExists_UpdatesStatusOnly()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Task", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var statusRequest = new TodoCompletionRequest(true);
        var response = await client.PatchAsJsonAsync($"/todos/{created!.Id}/status", statusRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.True(updated.IsComplete);
        Assert.Equal("Task", updated.Name);
    }

    [Fact]
    public async Task PatchTodoStatus_ToFalse_MarksIncomplete()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("Task", true));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var statusRequest = new TodoCompletionRequest(false);
        var response = await client.PatchAsJsonAsync($"/todos/{created!.Id}/status", statusRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.False(updated.IsComplete);
    }

    [Fact]
    public async Task PatchTodoStatus_WhenNotExists_ReturnsNotFound()
    {
        var client = CreateClient();
        var statusRequest = new TodoCompletionRequest(true);
        var response = await client.PatchAsJsonAsync("/todos/999/status", statusRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DELETE /todos/{id} Tests

    [Fact]
    public async Task DeleteTodo_WhenExists_ReturnsNoContent()
    {
        var client = CreateClient();
        var createResponse = await client.PostAsJsonAsync("/todos", new TodoCreateRequest("To Delete", false));
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        var response = await client.DeleteAsync($"/todos/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await client.GetAsync($"/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_WhenNotExists_ReturnsNotFound()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync("/todos/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
