using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Pages;

public sealed class ErrorModel : PageModel
{
    // The shared error page is intentionally generic and needs no request-specific data.
    public void OnGet()
    {
    }
}