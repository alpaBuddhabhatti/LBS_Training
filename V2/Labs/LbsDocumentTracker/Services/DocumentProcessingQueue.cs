using System.Threading.Channels;

namespace LbsDocumentTracker.Services;

public sealed class DocumentProcessingQueue
{
    // Bound memory use and apply backpressure when one worker is temporarily busy.
    private readonly Channel<QueuedDocument> _channel = Channel.CreateBounded<QueuedDocument>(
        new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    private int _accepting = 1;
    private int _workerStarted;

    // Readiness requires both an accepting queue and a started background worker.
    public bool IsReady => Volatile.Read(ref _accepting) == 1 && Volatile.Read(ref _workerStarted) == 1;

    // TryWrite fails rather than waiting when the queue is full or shutting down.
    public bool TryEnqueue(QueuedDocument document) =>
        Volatile.Read(ref _accepting) == 1 && _channel.Writer.TryWrite(document);

    public IAsyncEnumerable<QueuedDocument> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    public bool TryDequeue(out QueuedDocument? document) => _channel.Reader.TryRead(out document);

    public void MarkWorkerStarted() => Volatile.Write(ref _workerStarted, 1);

    public void MarkWorkerStopped() => Volatile.Write(ref _workerStarted, 0);

    public void StopAccepting()
    {
        // Complete the channel so the worker can finish reading items during shutdown.
        Volatile.Write(ref _accepting, 0);
        _channel.Writer.TryComplete();
    }
}