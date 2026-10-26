using LbsDocumentTracker;
using LbsDocumentTracker.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<LocalDocumentStore>();
builder.Services.AddSingleton<DocumentProcessingQueue>();
builder.Services.AddHostedService<DocumentProcessingWorker>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "LBS Document Tracker is a local-only starter and cannot run outside Development. " +
        "Replace its in-memory store and development-only access model with approved persistent storage and tenant authentication before hosting.");
}

app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapGet("/healthz", () => Results.Json(new { status = "ok" }));
app.MapGet("/readyz", (LocalDocumentStore store, DocumentProcessingQueue queue) =>
    store.IsAvailable && queue.IsReady
        ? Results.Json(new { status = "ready", mode = "local-memory" })
        : Results.Json(new { status = "not-ready" }, statusCode: StatusCodes.Status503ServiceUnavailable));
app.MapRazorPages();

app.Run();