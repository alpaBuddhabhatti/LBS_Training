namespace LbsDocumentTracker.Services;

// Snapshot of one submission's visible state; records are kept only in the local store.
public sealed record DocumentRecord(
    Guid Id,
    string Reference,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt = null,
    string? ResultSummary = null,
    string? Error = null);