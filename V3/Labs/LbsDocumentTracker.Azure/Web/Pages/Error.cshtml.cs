using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Azure.Web.Pages;

[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorModel : PageModel
{
    public string RequestId { get; private set; } = string.Empty;

    public void OnGet() => SetError();
    public void OnPost() => SetError();

    private void SetError()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
