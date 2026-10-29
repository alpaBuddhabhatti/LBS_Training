using LbsDocumentTracker.Azure.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Azure.Web.Pages;

public sealed class IndexModel(IDocumentRepository repository) : PageModel
{
    public IReadOnlyList<DocumentRecord> Documents { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Documents = await repository.GetRecentAsync(cancellationToken);
}
