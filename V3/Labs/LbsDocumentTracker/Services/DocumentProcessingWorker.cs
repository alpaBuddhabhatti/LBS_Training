namespace LbsDocumentTracker.Services;

public sealed class DocumentProcessingWorker(
    DocumentProcessingQueue queue,
    LocalDocumentStore store,
    ILogger<DocumentProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start readiness only once the hosted service begins consuming jobs.
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
            // Finish jobs already accepted before marking the worker unavailable.
            while (queue.TryDequeue(out var pending) && pending is not null)
            {
                await ProcessAsync(pending, CancellationToken.None);
            }

            queue.MarkWorkerStopped();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Reject new uploads before asking the hosted service to stop.
        queue.StopAccepting();
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(QueuedDocument item, CancellationToken cancellationToken)
    {
        try
        {
            // Status changes make the queue-to-result lifecycle visible on the detail page.
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
                // Invalid training fields are a review outcome, not a worker failure.
                store.MarkNeedsReview(item.Id, result.Summary);
                logger.LogWarning("Training document requires review for {DocumentId}.", item.Id);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Record interruption separately from a business-validation result.
            store.MarkFailed(item.Id, "Processing was interrupted during local shutdown.");
        }
        catch (Exception exception)
        {
            store.MarkFailed(item.Id, "The local demo could not process this document.");
            logger.LogError(exception, "Local document processing failed for {DocumentId}.", item.Id);
        }
    }

}