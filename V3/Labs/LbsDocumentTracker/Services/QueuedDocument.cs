namespace LbsDocumentTracker.Services;

// The queue carries the upload text in memory until the worker validates it.
public sealed record QueuedDocument(Guid Id, string Reference, string Text);