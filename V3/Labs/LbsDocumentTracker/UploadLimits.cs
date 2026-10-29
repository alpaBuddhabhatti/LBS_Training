namespace LbsDocumentTracker;

public static class UploadLimits
{
    // The total request allowance is larger to leave room for multipart form headers.
    public const int MaxFileBytes = 32 * 1024;
    public const int MaxRequestBytes = 40 * 1024;
}