namespace LbsDocumentTracker.Services;

public sealed class DocumentProcessingWorker(
    DocumentProcessingQueue queue,
    LocalDocumentStore store,
    ILogger<DocumentProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        queue.MarkWorkerStarted();
        try
        {
            await foreach (var document in queue.ReadAllAsync(stoppingToken))
            {
                await ProcessAsync(document, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            while (queue.TryDequeue(out var pending) && pending is not null)
            {
                await ProcessAsync(pending, CancellationToken.None);
            }

            queue.MarkWorkerStopped();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        queue.StopAccepting();
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(QueuedDocument item, CancellationToken cancellationToken)
    {
        try
        {
            store.MarkProcessing(item.Id);
            var result = TrainingDocumentValidator.Validate(item.Reference, item.Text);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Accepted)
            {
                store.MarkCompleted(item.Id, result.Summary);
                logger.LogInformation("Training document validation completed for {DocumentId}.", item.Id);
            }
            else
            {
                store.MarkNeedsReview(item.Id, result.Summary);
                logger.LogWarning("Training document requires review for {DocumentId}.", item.Id);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            store.MarkFailed(item.Id, "Processing was interrupted during local shutdown.");
        }
        catch (Exception exception)
        {
            store.MarkFailed(item.Id, "The local demo could not process this document.");
            logger.LogError(exception, "Local document processing failed for {DocumentId}.", item.Id);
        }
    }

}