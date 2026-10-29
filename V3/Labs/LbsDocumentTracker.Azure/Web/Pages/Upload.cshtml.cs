using LbsDocumentTracker.Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace LbsDocumentTracker.Azure.Web.Pages;

public sealed class UploadModel(IDocumentRepository repository, ILogger<UploadModel> logger) : PageModel
{
    [BindProperty]
    public string Reference { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Upload { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Reference = Reference?.Trim() ?? string.Empty;
        if (!UploadValidation.IsValidReference(Reference))
        {
            ModelState.AddModelError(nameof(Reference), "Use the fictional format LBS-DEMO-1234.");
        }

        if (Upload is null || Upload.Length == 0)
        {
            ModelState.AddModelError(nameof(Upload), "Choose a non-empty .txt file.");
        }
        else if (!string.Equals(Path.GetExtension(Upload.FileName), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(Upload), "Only .txt files are accepted.");
        }
        else if (Upload.Length > UploadValidation.MaxFileBytes)
        {
            ModelState.AddModelError(nameof(Upload), "The file must be 32 KiB or smaller.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string text;
        try
        {
            await using var input = Upload!.OpenReadStream();
            text = await UploadValidation.ReadAsync(input, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(nameof(Upload), exception.Message);
            return Page();
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not read the training upload.");
            ModelState.AddModelError(nameof(Upload), "The file could not be read. Choose it again.");
            return Page();
        }

        try
        {
            var id = await repository.SubmitAsync(Reference, text, cancellationToken);
            logger.LogInformation("Stored training document {DocumentId} and its durable dispatch intent.", id);
            return RedirectToPage("/Documents/Details", new { id });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "SQL submission failed. No success is being reported.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ModelState.AddModelError(string.Empty,
                "SQL could not confirm the submission. Check the dashboard before retrying to avoid duplicate submissions.");
            return Page();
        }
    }
}
