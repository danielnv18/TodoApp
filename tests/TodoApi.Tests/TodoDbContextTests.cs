using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Tests;

public class TodoDbContextTests
{
    private TodoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        return new TodoDbContext(options);
    }

    [Fact]
    public async Task TodoDbContext_CanAddAndRetrieveTodo()
    {
        // Arrange
        await using var context = CreateContext();
        var todo = new Todo
        {
            Name = "Test Task",
            IsComplete = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Act
        context.Todos.Add(todo);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Todos.FirstOrDefaultAsync();
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Task");
    }

    [Fact]
    public async Task TodoDbContext_EnforcesMaxLength()
    {
        // Arrange
        await using var context = CreateContext();
        
        // Act - verify default value is configured
        var entityType = context.Model.FindEntityType(typeof(Todo));
        var nameProperty = entityType!.FindProperty(nameof(Todo.Name));

        // Assert
        nameProperty!.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public async Task TodoDbContext_SetsDefaultIsComplete()
    {
        // Arrange
        await using var context = CreateContext();
        
        // Act - verify default value is configured
        var entityType = context.Model.FindEntityType(typeof(Todo));
        var isCompleteProperty = entityType!.FindProperty(nameof(Todo.IsComplete));

        // Assert
        isCompleteProperty!.GetDefaultValue().Should().Be(false);
    }

    [Fact]
    public async Task TodoDbContext_UpdateTodo_SavesChanges()
    {
        // Arrange
        await using var context = CreateContext();
        var todo = new Todo { Name = "Original", CreatedAtUtc = DateTime.UtcNow };
        context.Todos.Add(todo);
        await context.SaveChangesAsync();

        // Act
        todo.Name = "Updated";
        await context.SaveChangesAsync();

        // Assert
        var updated = await context.Todos.FirstAsync();
        updated.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task TodoDbContext_DeleteTodo_RemovesFromDatabase()
    {
        // Arrange
        await using var context = CreateContext();
        var todo = new Todo { Name = "To Delete", CreatedAtUtc = DateTime.UtcNow };
        context.Todos.Add(todo);
        await context.SaveChangesAsync();

        // Act
        context.Todos.Remove(todo);
        await context.SaveChangesAsync();

        // Assert
        var count = await context.Todos.CountAsync();
        count.Should().Be(0);
    }
}
