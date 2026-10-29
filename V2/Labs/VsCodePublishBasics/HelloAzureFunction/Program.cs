using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

// The isolated worker runs the C# function code; Core Tools supplies the local Functions host.
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
