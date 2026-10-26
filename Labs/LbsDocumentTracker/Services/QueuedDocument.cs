namespace LbsDocumentTracker.Services;

public sealed record QueuedDocument(Guid Id, string Reference, string Text);