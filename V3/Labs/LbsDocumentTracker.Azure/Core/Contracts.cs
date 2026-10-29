namespace LbsDocumentTracker.Azure.Core;

public sealed record DocumentRecord(
    Guid Id, string Reference, string Status, DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt, string? ResultSummary);

public sealed record PendingDocument(Guid Id, string Reference, string Text, bool IsCompleted);
public sealed record OutboxEntry(Guid Id, Guid DocumentId);

public interface IDocumentRepository
{
    Task<Guid> SubmitAsync(string reference, string text, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentRecord>> GetRecentAsync(CancellationToken cancellationToken);
    Task<DocumentRecord?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PendingDocument?> GetPendingAsync(Guid id, CancellationToken cancellationToken);
    Task CompleteAsync(Guid id, bool accepted, string summary, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxEntry>> GetOutboxAsync(CancellationToken cancellationToken);
    Task MarkDispatchedAsync(Guid outboxId, CancellationToken cancellationToken);
}

public interface IDocumentMessageSender
{
    Task SendAsync(OutboxEntry entry, CancellationToken cancellationToken);
}
