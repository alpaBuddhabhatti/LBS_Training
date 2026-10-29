using LbsDocumentTracker.Services;

namespace LbsDocumentTracker.Azure.Core;

public sealed class DocumentProcessor(IDocumentRepository repository)
{
    public async Task ProcessAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await repository.GetPendingAsync(documentId, cancellationToken)
            ?? throw new InvalidDataException($"Document {documentId} was not found.");
        if (document.IsCompleted)
        {
            return;
        }

        var result = TrainingDocumentValidator.Validate(document.Reference, document.Text);
        // The repository conditionally finalizes once, so concurrent redelivery is safe.
        await repository.CompleteAsync(document.Id, result.Accepted, result.Summary, cancellationToken);
    }
}

public sealed class OutboxDispatcher(IDocumentRepository repository, IDocumentMessageSender sender)
{
    public async Task<int> DispatchAsync(CancellationToken cancellationToken)
    {
        var entries = await repository.GetOutboxAsync(cancellationToken);
        foreach (var entry in entries)
        {
            // Mark only after broker acceptance. A crash here can redeliver, never lose the job.
            await sender.SendAsync(entry, cancellationToken);
            await repository.MarkDispatchedAsync(entry.Id, cancellationToken);
        }

        return entries.Count;
    }
}
