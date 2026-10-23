namespace MyHtmxApp.Models;

/// <summary>
/// Represents a single task in our task management domain.
/// This is intentionally simple—we want to focus on UI patterns, not domain complexity.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The task's title/description.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Whether the task has been completed.
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// When the task was created (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}