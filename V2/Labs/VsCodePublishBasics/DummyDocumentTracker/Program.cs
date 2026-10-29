var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve wwwroot/index.html and its fixed JSON/assets; there is no database or Function call.
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Only same-origin scripts/styles are allowed; keep JavaScript in the external file.
        context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self'; " +
            "object-src 'none'; base-uri 'none'; frame-ancestors 'none'";
    }
});

app.Run();
