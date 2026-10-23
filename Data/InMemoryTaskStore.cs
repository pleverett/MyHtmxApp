using MyHtmxApp.Models;

namespace MyHtmxApp.Data;

/// <summary>
/// A simple in-memory store for TaskItems.
///
/// Design notes:
/// - Static class for simplicity in a workshop context
/// - In production, you would use Entity Framework Core, Dapper, or another data access approach
/// - Data is lost when the application restarts—this is intentional for workshop isolation
/// </summary>
public static class InMemoryTaskStore
{
    // Auto-incrementing ID counter
    private static int _nextId = 1;

    // The actual storage - a simple list
    private static readonly List<TaskItem> _tasks = new();

    /// <summary>
    /// Returns all tasks, ordered by creation date (newest first).
    /// Returns a new list to prevent external modification of internal state.
    /// </summary>
    public static IReadOnlyList<TaskItem> All() => _tasks
        .OrderByDescending(t => t.CreatedUtc)
        .ToList();

    /// <summary>
    /// Adds a new task with the given title.
    /// Automatically assigns an ID and creation timestamp.
    /// </summary>
    /// <param name="title">The task title (will be trimmed)</param>
    /// <returns>The newly created TaskItem</returns>
    public static TaskItem Add(string title)
    {
        var item = new TaskItem
        {
            Id = _nextId++,
            Title = title.Trim(),
            IsDone = false,
            CreatedUtc = DateTime.UtcNow
        };
        _tasks.Add(item);
        return item;
    }
}
