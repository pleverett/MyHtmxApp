using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyHtmxApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult  OnGetHello()
    {
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return Content("<strong>Hello, htmx!</strong>", "text/html");
        }

        return Page();
    }
}
