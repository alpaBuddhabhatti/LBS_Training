using LbsDocumentTracker.Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Azure.Web.Pages.Documents;

public sealed class DetailsModel(IDocumentRepository repository) : PageModel
{
    public DocumentRecord? Document { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Document = await repository.GetAsync(id, cancellationToken);
        return Document is null ? NotFound() : Page();
    }
}
