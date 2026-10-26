using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Pages.Documents;

public sealed class DetailsModel(LocalDocumentStore store) : PageModel
{
    public DocumentRecord? Document { get; private set; }

    public IActionResult OnGet(Guid id)
    {
        if (!store.TryGet(id, out var document))
        {
            return NotFound();
        }

        Document = document;
        return Page();
    }
}