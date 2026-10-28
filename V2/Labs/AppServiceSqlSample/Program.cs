using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
// Read configuration rather than embedding credentials. The connection string selects
// local Windows authentication or the approved Azure Entra authentication route.
var connectionString = builder.Configuration.GetConnectionString("TrainingDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure the ConnectionStrings:TrainingDatabase setting before starting the sample.");
}

var app = builder.Build();

// Keep diagnostic details in server logs; return a generic problem response to the caller.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("UnhandledRequest");

    logger.LogError(exception, "The sample request failed. TraceId: {TraceId}", context.TraceIdentifier);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await Results.Problem(
        title: "Request failed",
        detail: "The sample could not complete this request. Ask the instructor to check the lab diagnostics.",
        statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
}));

app.MapGet("/", () => Results.Content(
    """
    <!doctype html>
    <html lang="en">
    <head><meta charset="utf-8"><title>App Service + Azure SQL lab</title></head>
    <body>
      <main>
        <h1>App Service + Azure SQL lab</h1>
        <p>This sample reads training rows from Azure SQL using Microsoft Entra authentication.</p>
        <p><a href="/api/items">View the sample data (JSON)</a></p>
        <p><a href="/health">Check that the web app is responding</a></p>
      </main>
    </body>
    </html>
    """,
    "text/html"));

// This checks the web process only. Use /api/items to verify database access.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/items", async (CancellationToken cancellationToken) =>
{
    var items = new List<TrainingItem>();

    // Each request opens and disposes its own connection; cancellation follows the HTTP request.
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync(cancellationToken);

    // The sample is read-only and selects explicit columns in a predictable order.
    await using var command = new SqlCommand(
        """
        SELECT Id, Name, Description
        FROM dbo.TrainingItems
        ORDER BY Id;
        """,
        connection)
    {
        CommandTimeout = 15
    };

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
        // Reader positions match Id, Name and Description in the SELECT above.
        items.Add(new TrainingItem(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)));
    }

    return Results.Ok(items);
});

app.Run();

internal sealed record TrainingItem(int Id, string Name, string Description);
