using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using LbsDocumentTracker.Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var server = configuration["Sql:Server"]
            ?? throw new InvalidOperationException("Configure Sql__Server.");
        var database = configuration["Sql:Database"]
            ?? throw new InvalidOperationException("Configure Sql__Database.");
        var serviceBusNamespace = configuration["ServiceBusConnection:fullyQualifiedNamespace"]
            ?? throw new InvalidOperationException("Configure ServiceBusConnection__fullyQualifiedNamespace.");
        var queueName = configuration["QueueName"]
            ?? throw new InvalidOperationException("Configure QueueName.");
        TokenCredential credential = string.Equals(
            configuration["AZURE_FUNCTIONS_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase)
            ? new AzureCliCredential()
            : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
        services.AddSingleton<IDocumentRepository>(new SqlDocumentRepository(server, database, credential));
        services.AddSingleton(_ => new ServiceBusClient(serviceBusNamespace, credential));
        services.AddSingleton(provider =>
            provider.GetRequiredService<ServiceBusClient>().CreateSender(queueName));
        services.AddSingleton<IDocumentMessageSender, DocumentMessageSender>();
        services.AddSingleton<OutboxDispatcher>();
        services.AddSingleton<DocumentProcessor>();
    })
    .Build();

await host.RunAsync();
