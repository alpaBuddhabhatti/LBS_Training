using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace HelloAzureFunction;

public sealed class ApplicationStatus
{
    [Function(nameof(ApplicationStatus))]
    public async Task<HttpResponseData> Run(
        // Azure requires a Function key; local Core Tools normally does not enforce it.
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request)
    {
        // This fixed fictional response does not query SQL, read blobs or process a document.
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            reference = "LBS-DEMO-1001",
            status = "Received",
            fictionalData = true,
            message = "Hello from the VS Code publishing exercise."
        });
        return response;
    }
}
