using System.Net;
using Azure.Identity;
using LbsDocumentTracker.Azure.Core;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "This Azure-backed training UI has no sign-in and must run locally in Development. " +
        "Implement tenant authentication and authorization before hosting it.");
}

// Keep the unauthenticated training UI on loopback even when URL environment variables are set.
builder.WebHost.UseUrls("http://127.0.0.1:5293");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = UploadValidation.MaxRequestBytes);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = UploadValidation.MaxRequestBytes;
    options.ValueCountLimit = 8;
    options.KeyLengthLimit = 128;
    options.ValueLengthLimit = 1024;
});
builder.Services.AddRazorPages();
var server = builder.Configuration["Sql:Server"]
    ?? throw new InvalidOperationException("Configure Sql:Server.");
var database = builder.Configuration["Sql:Database"]
    ?? throw new InvalidOperationException("Configure Sql:Database.");
builder.Services.AddSingleton<IDocumentRepository>(
    new SqlDocumentRepository(server, database, new AzureCliCredential()));

var app = builder.Build();
app.UseExceptionHandler("/Error");
app.Use(async (context, next) =>
{
    if (context.Connection.RemoteIpAddress is not { } address || !IPAddress.IsLoopback(address))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next(context);
});
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
