namespace TodoApi.Models;

/// <summary>
/// Represents a todo item in the system.
/// </summary>
public class Todo
{
    public int Id { get; set; }

    /// <summary>
    /// Short descriptive name for the todo (required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Flag indicating completion status.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Timestamp of creation in UTC for ordering/audit.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
