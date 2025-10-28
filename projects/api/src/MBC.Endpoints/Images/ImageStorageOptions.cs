namespace MBC.Endpoints.Images;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public long MaxFileSizeBytes { get; set; } = 1_048_576;
    public FileExtension[] AllowedExtensions { get; set; } = [
        FileExtension.From(".jpg"),
        FileExtension.From(".jpeg"),
        FileExtension.From(".png"),
        FileExtension.From(".webp")
    ];
    public MimeType[] AllowedMimeTypes { get; set; } = [
        MimeType.ImageJpeg,
        MimeType.ImagePng,
        MimeType.ImageWebP
    ];
    public string UploadDirectory { get; set; } = "assets/uploads";
}

