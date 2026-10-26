namespace LbsDocumentTracker.Services;

public sealed record DocumentValidationResult(bool Accepted, string Summary);

public static class TrainingDocumentValidator
{
    public static DocumentValidationResult Validate(string reference, string text)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var required = new[] { "FictionalData", "Reference", "Applicant", "DocumentType" };
        using var reader = new StringReader(text.TrimStart('\uFEFF'));
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            var separator = line.IndexOf(':');
            if (separator < 1)
            {
                return new(false, "Header lines must use Field: value, followed by a blank line.");
            }

            var key = line[..separator].Trim();
            if (!required.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                return new(false, "The document header contains an unsupported field.");
            }

            if (!fields.TryAdd(key, line[(separator + 1)..].Trim()))
            {
                return new(false, "The document header contains a duplicate field.");
            }
        }

        if (required.Any(key => !fields.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)))
        {
            return new(false, "Required fields: FictionalData, Reference, Applicant and DocumentType.");
        }

        if (!string.Equals(fields["FictionalData"], "true", StringComparison.OrdinalIgnoreCase))
        {
            return new(false, "The training document must declare FictionalData: true.");
        }

        if (!string.Equals(reference, fields["Reference"], StringComparison.Ordinal))
        {
            return new(false, "The document reference does not match the submission reference.");
        }

        if (fields["DocumentType"] is not ("ApplicationSummary" or "SupportingStatement"))
        {
            return new(false, "Supported document types: ApplicationSummary and SupportingStatement.");
        }

        if (string.IsNullOrWhiteSpace(reader.ReadToEnd()))
        {
            return new(false, "A non-empty fictional document body is required after the header.");
        }

        return new(true, "Training format validated. No identity, credit or regulatory checks were performed.");
    }
}
