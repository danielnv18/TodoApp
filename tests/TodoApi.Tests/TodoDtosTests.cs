using Xunit;
using FluentAssertions;
using TodoApi.Contracts;

namespace TodoApi.Tests;

public class TodoDtosTests
{
    [Fact]
    public void TodoCreateRequest_ValidData_CreatesCorrectly()
    {
        // Act
        var dto = new TodoCreateRequest("Test", true);

        // Assert
        dto.Name.Should().Be("Test");
        dto.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TodoCreateRequest_DefaultIsComplete_IsFalse()
    {
        // Act
        var dto = new TodoCreateRequest("Test");

        // Assert
        dto.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void TodoUpdateRequest_ValidData_CreatesCorrectly()
    {
        // Act
        var dto = new TodoUpdateRequest("Updated", true);

        // Assert
        dto.Name.Should().Be("Updated");
        dto.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TodoResponse_AllProperties_SetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new TodoResponse(1, "Task", true, now);

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Task");
        dto.IsComplete.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void TodoCompletionRequest_SetStatus_CreatesCorrectly()
    {
        // Act
        var dto = new TodoCompletionRequest(true);

        // Assert
        dto.IsComplete.Should().BeTrue();
    }
}
