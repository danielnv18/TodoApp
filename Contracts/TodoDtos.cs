using System.ComponentModel.DataAnnotations;

namespace TodoApi.Contracts;

/// <summary>
/// Request payload for creating a new todo.
/// </summary>
public record TodoCreateRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    bool IsComplete = false
);

/// <summary>
/// Request payload for updating an existing todo.
/// </summary>
public record TodoUpdateRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    bool IsComplete
);

/// <summary>
/// API response shape for returning todo data.
/// </summary>
public record TodoResponse(
    int Id,
    string Name,
    bool IsComplete,
    DateTime CreatedAtUtc
);
