using System.Text;
using System.Text.Json;
using LbsDocumentTracker.Azure.Core;
using LbsDocumentTracker.Azure.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;

if (args.Length != 1)
{
    throw new ArgumentException("Pass the absolute SampleData directory.");
}

var root = Path.GetFullPath(args[0]);
var cases = JsonSerializer.Deserialize<SampleCase[]>(
    await File.ReadAllTextAsync(Path.Combine(root, "cases.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidDataException("Missing sample cases.");
if (cases.Length != 10)
{
    throw new InvalidDataException("Expected all ten training sample cases.");
}

foreach (var sample in cases)
{
    var repository = new MemoryRepository();
    var text = await File.ReadAllTextAsync(Path.Combine(root, sample.File));
    var id = await repository.SubmitAsync(sample.Reference, text, default);
    var sender = new RecordingSender();
    var dispatcher = new OutboxDispatcher(repository, sender);
    Equal(1, await dispatcher.DispatchAsync(default), "one job dispatched");
    Equal(id, sender.Sent.Single().DocumentId, "correct document dispatched");
    Equal(0, await dispatcher.DispatchAsync(default), "dispatched job not resent");
    var processor = new DocumentProcessor(repository);
    await processor.ProcessAsync(id, default);
    Equal(sample.ExpectedStatus, repository.Document!.Status, sample.File);
    var completedAt = repository.Document.CompletedAt;
    await processor.ProcessAsync(id, default);
    Equal(1, repository.Completions, "redelivery is idempotent");
    Equal(completedAt, repository.Document.CompletedAt, "completion timestamp is stable");
    Equal(string.Empty, repository.Text, "processed content cleared");
    Console.WriteLine($"PASS pipeline: {sample.File} -> {sample.ExpectedStatus}");
}

var valid = await File.ReadAllTextAsync(Path.Combine(root, "01-valid-application.txt"));
var failureRepository = new MemoryRepository();
var failureId = await failureRepository.SubmitAsync("LBS-DEMO-1001", valid, default);
var failureSender = new RecordingSender { Fail = true };
var failureDispatcher = new OutboxDispatcher(failureRepository, failureSender);
await ThrowsAsync<IOException>(() => failureDispatcher.DispatchAsync(default), "broker failure propagates");
Equal(1, (await failureRepository.GetOutboxAsync(default)).Count, "broker failure retains outbox");
failureSender.Fail = false;
failureRepository.FailMarkDispatched = true;
await ThrowsAsync<IOException>(() => failureDispatcher.DispatchAsync(default), "post-send SQL failure propagates");
Equal(1, (await failureRepository.GetOutboxAsync(default)).Count, "post-send failure retains outbox");
failureRepository.FailMarkDispatched = false;
await failureDispatcher.DispatchAsync(default);
Equal(2, failureSender.Sent.Count, "retry can redeliver after broker acceptance");
Equal(failureSender.Sent[0].Id, failureSender.Sent[1].Id, "retry preserves message identifier");
var failureProcessor = new DocumentProcessor(failureRepository);
await failureProcessor.ProcessAsync(failureId, default);
await failureProcessor.ProcessAsync(failureId, default);
Equal(1, failureRepository.Completions, "outbox redelivery finalizes once");
await ThrowsAsync<InvalidDataException>(
    () => failureProcessor.ProcessAsync(Guid.NewGuid(), default), "unknown document is not silently accepted");

var message = new DocumentMessage(1, Guid.NewGuid());
Equal(message, DocumentMessage.Parse(JsonSerializer.Serialize(message)), "message round-trip");
foreach (var json in new[] { "{}", "null", """{"Version":2,"DocumentId":"00000000-0000-0000-0000-000000000001"}""" })
{
    await ThrowsAsync<InvalidDataException>(() => Task.FromResult(DocumentMessage.Parse(json)), "invalid message");
}
await ThrowsAsync<JsonException>(() => Task.FromResult(DocumentMessage.Parse("{")), "malformed JSON");

True(UploadValidation.IsValidReference("LBS-DEMO-1001"), "valid reference");
True(!UploadValidation.IsValidReference("LBS-DEMO-1001\n"), "reference newline rejected");
True(!UploadValidation.IsValidReference("LBS-DEMO-12345"), "long reference rejected");
True(!UploadValidation.IsValidReference("REAL-1001"), "non-training reference rejected");
await ReadTextAsync(Encoding.UTF8.GetBytes(valid));
await ReadTextAsync(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(valid)).ToArray());
await ReadTextAsync(Enumerable.Repeat((byte)'x', UploadValidation.MaxFileBytes).ToArray());
await ThrowsAsync<InvalidDataException>(
    () => ReadTextAsync(new byte[UploadValidation.MaxFileBytes + 1]), "over limit");
await ThrowsAsync<InvalidDataException>(() => ReadTextAsync([0xff, 0xff]), "invalid UTF-8");
await ThrowsAsync<InvalidDataException>(() => ReadTextAsync([]), "empty text");
await ThrowsAsync<InvalidDataException>(() => ReadTextAsync(Encoding.UTF8.GetBytes(" \r\n\t")), "whitespace");
await ThrowsAsync<InvalidDataException>(() => ReadTextAsync([65, 0, 66]), "control character");
Console.WriteLine("PASS strict upload decoding and exact 32 KiB boundary");

await TestUploadAsync("01-valid-application.txt", Encoding.UTF8.GetBytes(valid), true);
await TestUploadAsync("document.pdf", Encoding.UTF8.GetBytes(valid), false);
await TestUploadAsync("empty.txt", [], false);
await TestUploadAsync("large.txt", new byte[UploadValidation.MaxFileBytes + 1], false);
await TestUploadAsync("invalid-encoding.txt", [0xff], false);
await TestUploadAsync("valid.txt", Encoding.UTF8.GetBytes(valid), false, "WRONG-REFERENCE");
await TestUploadAsync("valid.txt", Encoding.UTF8.GetBytes(valid), false, "LBS-DEMO-\n1001");
await TestUploadAsync("valid.txt", Encoding.UTF8.GetBytes(valid), true, "  LBS-DEMO-1001  ");
Console.WriteLine("PASS upload page validation and persistence handoff");
Console.WriteLine("ALL OFFLINE CHECKS PASSED. No Azure connection, SQL transaction, host binding or live delivery was tested.");

static async Task<string> ReadTextAsync(byte[] bytes)
{
    using var stream = new MemoryStream(bytes);
    return await UploadValidation.ReadAsync(stream, default);
}

static async Task TestUploadAsync(
    string filename, byte[] bytes, bool accepted, string reference = "LBS-DEMO-1001")
{
    var repository = new MemoryRepository();
    using var stream = new MemoryStream(bytes);
    var page = new UploadModel(repository, NullLogger<UploadModel>.Instance)
    {
        Reference = reference,
        Upload = new FormFile(stream, 0, bytes.Length, "Upload", filename),
        PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
    };
    var result = await page.OnPostAsync(default);
    Equal(accepted, result is RedirectToPageResult, $"upload result: {filename}");
    Equal(accepted, repository.Document is not null, $"upload persistence: {filename}");
    Equal(accepted, page.ModelState.IsValid, $"upload errors: {filename}");
}

static async Task ThrowsAsync<T>(Func<Task> action, string name) where T : Exception
{
    try
    {
        await action();
    }
    catch (T)
    {
        Console.WriteLine($"PASS failure handling: {name}");
        return;
    }

    throw new InvalidOperationException($"{name}: expected {typeof(T).Name}.");
}

static void Equal<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{name}: expected {expected}, got {actual}.");
    }
}

static void True(bool condition, string name) => Equal(true, condition, name);

internal sealed record SampleCase(string File, string Reference, string ExpectedStatus);

internal sealed class RecordingSender : IDocumentMessageSender
{
    public bool Fail { get; set; }
    public List<OutboxEntry> Sent { get; } = [];

    public Task SendAsync(OutboxEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Fail)
        {
            throw new IOException("Injected broker failure.");
        }

        Sent.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class MemoryRepository : IDocumentRepository
{
    public DocumentRecord? Document { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int Completions { get; private set; }
    public bool FailMarkDispatched { get; set; }
    private OutboxEntry? _outbox;

    public Task<Guid> SubmitAsync(string reference, string text, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        Document = new(id, reference, "Queued", DateTimeOffset.UtcNow, null, null);
        Text = text;
        _outbox = new(Guid.NewGuid(), id);
        return Task.FromResult(id);
    }

    public Task<IReadOnlyList<DocumentRecord>> GetRecentAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DocumentRecord>>(Document is { } document ? [document] : []);

    public Task<DocumentRecord?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Document?.Id == id ? Document : null);

    public Task<PendingDocument?> GetPendingAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Document is { } document && document.Id == id
            ? new PendingDocument(id, document.Reference, Text, document.CompletedAt is not null) : null);

    public Task CompleteAsync(Guid id, bool accepted, string summary, CancellationToken cancellationToken)
    {
        if (Document is { } document && document.Id == id && document.CompletedAt is null)
        {
            Document = document with
            {
                Status = accepted ? "Completed" : "Needs review",
                CompletedAt = DateTimeOffset.UtcNow,
                ResultSummary = summary
            };
            Text = string.Empty;
            Completions++;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxEntry>> GetOutboxAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<OutboxEntry>>(_outbox is { } entry ? [entry] : []);

    public Task MarkDispatchedAsync(Guid outboxId, CancellationToken cancellationToken)
    {
        if (FailMarkDispatched)
        {
            throw new IOException("Injected SQL acknowledgement failure.");
        }

        if (_outbox?.Id == outboxId)
        {
            _outbox = null;
        }

        return Task.CompletedTask;
    }

}
