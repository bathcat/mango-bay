namespace MBC.Core.Models;

public sealed class ImageUploadResult
{
    public bool Success { get; init; }
    public string? RelativePath { get; init; }
    public string? ErrorMessage { get; init; }
    public long FileSize { get; init; }

    public static ImageUploadResult SuccessResult(string relativePath, long fileSize) =>
        new()
        {
            Success = true,
            RelativePath = relativePath,
            FileSize = fileSize
        };

    public static ImageUploadResult FailureResult(string errorMessage) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
}

