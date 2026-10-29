using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Pages;

public sealed class IndexModel(LocalDocumentStore store) : PageModel
{
    public IReadOnlyList<DocumentRecord> Documents { get; private set; } = Array.Empty<DocumentRecord>();

    // Build the dashboard from the newest records still held by this running process.
    public void OnGet() => Documents = store.GetRecent();
}