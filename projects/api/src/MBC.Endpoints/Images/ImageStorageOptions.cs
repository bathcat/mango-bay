namespace MBC.Endpoints.Images;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public long MaxFileSizeBytes { get; set; } = 1_048_576;

    //TODO: Consider creating a value object to capture this.
    //Benefits:
    //  1. Readability
    //  2. Validation e.g. no one can configure an AllowedExtension like `..\..\..\.bmp
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public string[] AllowedMimeTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
    public string UploadDirectory { get; set; } = "assets/uploads";
}

