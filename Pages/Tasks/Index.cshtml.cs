using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

    // ═══════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the current request was made by htmx.
    /// htmx sends "HX-Request: true" header with every request.
    /// </summary>
    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

    /// <summary>
    /// Returns a partial view result for fragment responses.
    /// This helper creates a PartialViewResult with the correct ViewData context.
    /// </summary>
    /// <param name="partialName">Path to the partial view</param>
    /// <param name="model">Model to pass to the partial</param>
    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
        };

    /// <summary>
    /// Handles GET requests to /Tasks.
    /// Loads all tasks for display.
    /// </summary>
    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    // ═══════════════════════════════════════════════════════════
    // List Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Handles GET requests to /Tasks?handler=List.
    /// Returns just the task list fragment for htmx to swap.
    ///
    /// Optional parameter 'take' limits the number of tasks returned.
    /// </summary>
    /// <param name="take">Optional: limit results to this many tasks</param>
    public IActionResult OnGetList(int? take)
    {
        var tasks = InMemoryTaskStore.All();

        if (take is > 0)
        {
            tasks = tasks.Take(take.Value).ToList();
        }

        return Fragment("Partials/_TaskList", tasks);
    }

    // ═══════════════════════════════════════════════════════════
    // Form Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Returns an empty form fragment.
    /// Called via htmx trigger after successful task creation.
    /// </summary>
    public IActionResult OnGetEmptyForm()
    {
        Input = new NewTaskInput();
        ModelState.Clear();
        return Fragment("Partials/_TaskForm", this);
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

            if (IsHtmx())
            {
                // For htmx: return the form fragment with validation errors
                // Use response headers to retarget the swap to the form
                Response.Headers["Hx-Retarget"] = "#task-form";
                Response.Headers["Hx-Reswap"] = "outerHTML";
                return Fragment("Partials/_TaskForm", this);
            }

            FlashMessage = "Please correct the errors and try again";

            return Page();
        }

        // Simulated error condition for demonstration
        // Type "boom" as the title to trigger this error
        if (Input.Title.Trim().Equals("boom", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHtmx())
            {
                Response.Headers["HX-Retarget"] = "#messages";
                Response.Headers["HX-Reswap"] = "innerHTML";
                return Fragment("Partials/_Error",
                    "Simulated server error. Try a different title (anything except 'boom').");
            }

            throw new InvalidOperationException("Simulated server error.");
        }


        // Create the task
        InMemoryTaskStore.Add(Input.Title);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            // Trigger form clear after successful creation
            Response.Headers["HX-Trigger"] = "clearForm";
            return Fragment("Partials/_TaskList", Tasks);
        }

        // Set success message and redirect (PRG pattern)
        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    public IActionResult OnPostReset()
    {
        InMemoryTaskStore.Reset();
        FlashMessage = "Tasks reset.";
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