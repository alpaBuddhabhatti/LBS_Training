using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace LbsDocumentTracker.Azure.Core;

public sealed record DocumentMessage(int Version, Guid DocumentId)
{
    public static DocumentMessage Parse(string json)
    {
        var message = JsonSerializer.Deserialize<DocumentMessage>(json)
            ?? throw new InvalidDataException("A document message is required.");
        if (message.Version != 1 || message.DocumentId == Guid.Empty)
        {
            throw new InvalidDataException("Unsupported message version or invalid document identifier.");
        }

        return message;
    }
}

public sealed class DocumentMessageSender(ServiceBusSender sender) : IDocumentMessageSender
{
    public Task SendAsync(OutboxEntry entry, CancellationToken cancellationToken) =>
        sender.SendMessageAsync(new ServiceBusMessage(
            BinaryData.FromObjectAsJson(new DocumentMessage(1, entry.DocumentId)))
        {
            MessageId = entry.Id.ToString("D"),
            CorrelationId = entry.DocumentId.ToString("D"),
            ContentType = "application/json",
            Subject = "training-document-v1"
        }, cancellationToken);
}
