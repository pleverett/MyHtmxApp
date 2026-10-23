using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyHtmxApp.Data;
using MyHtmxApp.Models;

namespace MyHtmxApp.Pages.Tasks;

/// <summary>
/// PageModel for the Tasks index page.
///
/// This handles:
/// - Displaying the list of tasks (OnGet)
/// - Creating new tasks (OnPostCreate)
///
/// Design notes:
/// - Tasks property is populated on every request that needs to display the list
/// - Input property uses [BindProperty] for model binding on POST
/// - FlashMessage uses [TempData] to survive redirects
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// The list of tasks to display. Populated in handlers that render the page.
    /// </summary>
    public IReadOnlyList<TaskItem> Tasks { get; private set; } = Array.Empty<TaskItem>();

    /// <summary>
    /// Input model for creating new tasks. Bound automatically on POST requests.
    /// </summary>
    [BindProperty]
    public NewTaskInput Input { get; set; } = new();

    /// <summary>
    /// Flash message to display after redirects.
    /// TempData survives one redirect, making it perfect for "success" messages.
    /// </summary>
    [TempData]
    public string? FlashMessage { get; set; }

    /// <summary>
    /// Handles GET requests to /Tasks.
    /// Loads all tasks for display.
    /// </summary>
    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    /// <summary>
    /// Handles POST requests to /Tasks?handler=Create.
    /// Validates input, creates the task, and redirects back to the page.
    ///
    /// The Post-Redirect-Get (PRG) pattern prevents duplicate submissions
    /// when users refresh the page after submitting.
    /// </summary>
    public IActionResult OnPostCreate()
    {
        // Manual validation (we'll add data annotations in Lab 3)
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        // If validation failed, redisplay the page with errors
        if (!ModelState.IsValid)
        {
            Tasks = InMemoryTaskStore.All();
            return Page();
        }

        // Create the task
        InMemoryTaskStore.Add(Input.Title);

        // Set success message and redirect (PRG pattern)
        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    /// <summary>
    /// Input model for the create task form.
    /// Nested class keeps it close to its usage context.
    /// </summary>
    public class NewTaskInput
    {
        public string Title { get; set; } = "";
    }
}