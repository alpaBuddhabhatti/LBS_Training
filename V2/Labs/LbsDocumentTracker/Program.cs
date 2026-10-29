using LbsDocumentTracker;
using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Keep both server and form limits aligned with the demo's small upload allowance.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = UploadLimits.MaxRequestBytes;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = UploadLimits.MaxRequestBytes;
    options.ValueCountLimit = 8;
    options.KeyLengthLimit = 128;
    options.ValueLengthLimit = 1024;
});
builder.Services.AddRazorPages();
// These singleton services share one in-memory store and queue for this process.
builder.Services.AddSingleton<LocalDocumentStore>();
builder.Services.AddSingleton<DocumentProcessingQueue>();
// The hosted worker consumes queued uploads in the background.
builder.Services.AddHostedService<DocumentProcessingWorker>();

var app = builder.Build();

// This sample has no sign-in or durable storage, so refuse to run outside local development.
if (!app.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "LBS Document Tracker is a local-only starter and cannot run outside Development. " +
        "Replace its in-memory store and development-only access model with approved persistent storage and tenant authentication before hosting.");
}

app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
// Health checks distinguish a running web app from one whose local worker is ready.
app.MapGet("/healthz", () => Results.Json(new { status = "ok" }));
app.MapGet("/readyz", (LocalDocumentStore store, DocumentProcessingQueue queue) =>
    store.IsAvailable && queue.IsReady
        ? Results.Json(new { status = "ready", mode = "local-memory" })
        : Results.Json(new { status = "not-ready" }, statusCode: StatusCodes.Status503ServiceUnavailable));
app.MapRazorPages();

app.Run();