using System.Text;
using System.Text.RegularExpressions;

namespace LbsDocumentTracker.Azure.Core;

public static class UploadValidation
{
    public const int MaxFileBytes = 32 * 1024;
    public const int MaxRequestBytes = 40 * 1024;

    public static bool IsValidReference(string reference) =>
        Regex.IsMatch(reference, "\\ALBS-DEMO-[0-9]{4}\\z", RegexOptions.CultureInvariant);

    public static async Task<string> ReadAsync(Stream input, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = new byte[4096];
        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            if (output.Length + read > MaxFileBytes)
            {
                throw new InvalidDataException("The file must be 32 KiB or smaller.");
            }

            output.Write(buffer, 0, read);
        }

        string text;
        try
        {
            text = new UTF8Encoding(false, true).GetString(output.ToArray());
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("The file is not valid UTF-8 text.", exception);
        }

        if (string.IsNullOrWhiteSpace(text) || text.Any(character =>
                char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
        {
            throw new InvalidDataException("Use non-empty plain text without unsupported control characters.");
        }

        return text;
    }
}
