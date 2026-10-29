using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LbsDocumentTracker.Azure.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LbsDocumentTracker.Azure.Functions;

public sealed class DocumentFunctions(
    OutboxDispatcher dispatcher, DocumentProcessor processor, ILogger<DocumentFunctions> logger)
{
    [Function("DispatchTrainingDocuments")]
    public async Task DispatchAsync(
        [TimerTrigger("*/15 * * * * *")] TimerInfo timer, CancellationToken cancellationToken)
    {
        var count = await dispatcher.DispatchAsync(cancellationToken);
        logger.LogInformation("Dispatched {Count} training document jobs.", count);
    }

    [Function("ProcessTrainingDocument")]
    public async Task ProcessAsync(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection", AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions actions,
        CancellationToken cancellationToken)
    {
        DocumentMessage job;
        try
        {
            job = DocumentMessage.Parse(message.Body.ToString());
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        {
            logger.LogWarning("Dead-lettering malformed training message {MessageId}.", message.MessageId);
            await actions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidDocumentMessage",
                deadLetterErrorDescription: "Expected version 1 and a non-empty document identifier.",
                cancellationToken: cancellationToken);
            return;
        }

        // Technical errors propagate to the Functions host for retry and eventual dead-lettering.
        await processor.ProcessAsync(job.DocumentId, cancellationToken);
        await actions.CompleteMessageAsync(message, cancellationToken);
        logger.LogInformation("Processed training document {DocumentId}.", job.DocumentId);
    }
}
