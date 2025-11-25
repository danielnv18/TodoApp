using System;
using TodoApi.Models;

namespace TodoApi.Tests;

public class TodoModelTests
{
    [Xunit.Fact]
    public void Todo_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var todo = new Todo();

        // Assert
        Xunit.Assert.Equal(0, todo.Id);
        Xunit.Assert.Equal(string.Empty, todo.Name);
        Xunit.Assert.False(todo.IsComplete);
        Xunit.Assert.True((DateTime.UtcNow - todo.CreatedAtUtc) < TimeSpan.FromSeconds(2));
    }

    [Xunit.Fact]
    public void Todo_SetProperties_StoresCorrectly()
    {
        // Arrange
        var todo = new Todo();
        var now = DateTime.UtcNow;

        // Act
        todo.Id = 1;
        todo.Name = "Test Task";
        todo.IsComplete = true;
        todo.CreatedAtUtc = now;

        // Assert
        Xunit.Assert.Equal(1, todo.Id);
        Xunit.Assert.Equal("Test Task", todo.Name);
        Xunit.Assert.True(todo.IsComplete);
        Xunit.Assert.Equal(now, todo.CreatedAtUtc);
    }
}
