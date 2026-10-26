namespace LbsDocumentTracker.Services;

public sealed record DocumentRecord(
    Guid Id,
    string Reference,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt = null,
    string? ResultSummary = null,
    string? Error = null);