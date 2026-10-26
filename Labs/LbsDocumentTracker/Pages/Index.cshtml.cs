using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Pages;

public sealed class IndexModel(LocalDocumentStore store) : PageModel
{
    public IReadOnlyList<DocumentRecord> Documents { get; private set; } = Array.Empty<DocumentRecord>();

    public void OnGet() => Documents = store.GetRecent();
}