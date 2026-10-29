using System.Text.Json;
using LbsDocumentTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;

if (args.Length != 1)
{
    // The checks read the supplied sample directory rather than relying on a fixed working folder.
    throw new ArgumentException("Pass the absolute SampleData directory.");
}

var root = Path.GetFullPath(args[0]);
var cases = JsonSerializer.Deserialize<SampleCase[]>(
    await File.ReadAllTextAsync(Path.Combine(root, "cases.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidDataException("Missing sample cases.");
var store = new LocalDocumentStore();
var queue = new DocumentProcessingQueue();
using var worker = new DocumentProcessingWorker(queue, store, NullLogger<DocumentProcessingWorker>.Instance);
// Exercise the real background worker and in-memory services without starting the web host.
await worker.StartAsync(CancellationToken.None);
try
{
    foreach (var sample in cases)
    {
        // Poll briefly for the asynchronous worker to update each record.
        var document = store.Create(sample.Reference);
        var text = await File.ReadAllTextAsync(Path.Combine(root, sample.File));
        if (!queue.TryEnqueue(new QueuedDocument(document.Id, sample.Reference, text)))
        {
            throw new InvalidOperationException("Queue did not accept sample.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        DocumentRecord? actual;
        do
        {
            await Task.Delay(10, timeout.Token);
            store.TryGet(document.Id, out actual);
        } while (actual is null || actual.CompletedAt is null);
        if (actual.Status != sample.ExpectedStatus)
        {
            throw new InvalidOperationException($"{sample.File}: expected {sample.ExpectedStatus}, got {actual.Status}.");
        }

        Console.WriteLine($"PASS worker: {sample.File} -> {actual.Status}");
    }

    var valid = await File.ReadAllTextAsync(Path.Combine(root, "01-valid-application.txt"));
    // Probe validator edge cases in addition to the end-to-end worker examples.
    AssertAccepted("valid document", valid, true);
    AssertAccepted("UTF-8 BOM", "\uFEFF" + valid, true);
    AssertAccepted("duplicate field", valid.Replace("Applicant:", "Reference: LBS-DEMO-1001\nApplicant:"), false);
    AssertAccepted("non-fictional marker", valid.Replace("FictionalData: true", "FictionalData: false"), false);
    AssertAccepted("empty document", "", false);
    AssertAccepted("missing body", valid.Replace("\r\n", "\n").Split("\n\n")[0] + "\n\n", false);
    AssertAccepted("malformed field", valid.Replace("Applicant:", "Applicant"), false);
    AssertAccepted("unknown field", valid.Replace("Applicant:", "Unexpected:"), false);
    AssertAccepted("supporting statement", valid.Replace("ApplicationSummary", "SupportingStatement"), true);
}
finally
{
    // Always stop the worker so shutdown behavior is included in the check.
    using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    await worker.StopAsync(stopTimeout.Token);
}

if (queue.IsReady)
{
    throw new InvalidOperationException("Queue remained ready after stopping.");
}

Console.WriteLine("PASS worker shutdown");

static void AssertAccepted(string name, string text, bool expected)
{
    var result = TrainingDocumentValidator.Validate("LBS-DEMO-1001", text);
    if (result.Accepted != expected)
    {
        throw new InvalidOperationException($"{name}: unexpected validation outcome.");
    }

    Console.WriteLine($"PASS validation: {name}");
}

internal sealed record SampleCase(string File, string Reference, string ExpectedStatus);
