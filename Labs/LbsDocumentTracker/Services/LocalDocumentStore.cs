using System.Collections.Concurrent;

namespace LbsDocumentTracker.Services;

public sealed class LocalDocumentStore
{
    private readonly ConcurrentDictionary<Guid, DocumentRecord> _documents = new();

    public bool IsAvailable => true;

    public DocumentRecord Create(string reference)
    {
        var document = new DocumentRecord(Guid.NewGuid(), reference, "Queued", DateTimeOffset.UtcNow);
        _documents.TryAdd(document.Id, document);
        return document;
    }

    public bool TryGet(Guid id, out DocumentRecord? document) => _documents.TryGetValue(id, out document);

    public IReadOnlyList<DocumentRecord> GetRecent() => _documents.Values
        .OrderByDescending(document => document.CreatedAt)
        .Take(100)
        .ToArray();

    public void Remove(Guid id) => _documents.TryRemove(id, out _);

    public void MarkProcessing(Guid id) => Update(id, document => document with { Status = "Processing" });

    public void MarkCompleted(Guid id, string summary) => Update(id, document => document with
    {
        Status = "Completed",
        CompletedAt = DateTimeOffset.UtcNow,
        ResultSummary = summary
    });

    public void MarkFailed(Guid id, string message) => Update(id, document => document with
    {
        Status = "Failed",
        CompletedAt = DateTimeOffset.UtcNow,
        Error = message
    });

    public void MarkNeedsReview(Guid id, string summary) => Update(id, document => document with
    {
        Status = "Needs review",
        CompletedAt = DateTimeOffset.UtcNow,
        ResultSummary = summary
    });

    private void Update(Guid id, Func<DocumentRecord, DocumentRecord> update)
    {
        _documents.AddOrUpdate(id, _ => throw new InvalidOperationException("Document not found."), (_, current) => update(current));
    }
}