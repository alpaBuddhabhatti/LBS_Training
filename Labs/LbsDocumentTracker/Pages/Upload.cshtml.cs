using System.Text;
using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LbsDocumentTracker.Pages;

public sealed class UploadModel(
    LocalDocumentStore store,
    DocumentProcessingQueue queue,
    ILogger<UploadModel> logger) : PageModel
{
    [BindProperty]
    public string Reference { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Upload { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Reference = Reference.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(Reference, "\\ALBS-DEMO-[0-9]{4}\\z", System.Text.RegularExpressions.RegexOptions.CultureInvariant))
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
        else if (Upload.Length > UploadLimits.MaxFileBytes)
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
            await using var stream = Upload!.OpenReadStream();
            var bytes = await ReadBoundedAsync(stream, UploadLimits.MaxFileBytes, cancellationToken);
            text = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
            if (string.IsNullOrWhiteSpace(text) || text.Any(character =>
                    char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
            {
                ModelState.AddModelError(nameof(Upload), "The file must contain non-empty plain text without unsupported control characters.");
                return Page();
            }
        }
        catch (DecoderFallbackException)
        {
            ModelState.AddModelError(nameof(Upload), "The file is not valid UTF-8 text.");
            return Page();
        }
        catch (InvalidDataException)
        {
            ModelState.AddModelError(nameof(Upload), "The file must be 32 KiB or smaller.");
            return Page();
        }
        catch (IOException)
        {
            ModelState.AddModelError(nameof(Upload), "The file could not be read. Please choose it again.");
            return Page();
        }

        var document = store.Create(Reference);
        if (!queue.TryEnqueue(new QueuedDocument(document.Id, Reference, text)))
        {
            store.Remove(document.Id);
            ModelState.AddModelError(string.Empty, "The local processing queue is unavailable or full. Try again shortly.");
            return Page();
        }

        logger.LogInformation("Accepted fictional document {DocumentId} for local processing.", document.Id);
        return RedirectToPage("/Documents/Details", new { id = document.Id });
    }

    private static async Task<byte[]> ReadBoundedAsync(Stream input, int maximumBytes, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = new byte[4096];
        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                return output.ToArray();
            }

            if (output.Length + read > maximumBytes)
            {
                throw new InvalidDataException("Upload exceeds the permitted size.");
            }

            output.Write(buffer, 0, read);
        }
    }
}