# Images

Infrastructure for secure image upload handling with validation and file storage.

This folder implements the `IImageRepository` interface from `MBC.Core.Services` with filesystem-backed storage, including MIME type validation, size limits, and path traversal protection. Images are stored in separate directories based on visibility requirements: public site images vs. protected delivery proofs.

## What Belongs Here

-   File storage implementations for `IImageRepository` interface
-   MIME type detection via magic byte validation
-   File upload validation (size, extension, content type)
-   Path traversal protection and secure file access
-   DI registration and configuration binding for image storage
-   Static file middleware setup for serving uploaded images

## What Doesn't Belong Here

-   Endpoint route handlers (use `/Endpoints`)
-   Domain entities or business models (use `MBC.Core/Entities` or `MBC.Core/Models`)
-   Business rules about when to upload images (use service layer in `MBC.Services`)
-   Image processing or transformation logic

## Key Concepts

**Magic Byte Validation**: Validates actual file content by reading binary headers, not just file extensions. Prevents attackers from renaming malicious files with image extensions. The `MimeTypes` class checks for JPEG (FF D8 FF), PNG (89 50 4E 47...), and WebP (52 49 46 46...) signatures.

**Dual Storage Zones**:

-   **Public** (`assets/uploads`): Publicly accessible images like site photos, served via static file middleware at `/uploads`
-   **Protected** (`assets/protected`): Sensitive images like proof of delivery, requiring authorization to access

**Configuration-Driven Security**: File size limits, allowed extensions, and MIME types configured in `appsettings.json` under `ImageStorage` section, allowing easy security policy adjustments.

## Usage

**Register in `Program.cs`:**

```csharp
builder.AddImageStorage();

app.UseImageStaticFiles();
```

**Inject and use in endpoints or services:**

```csharp
public class SomeEndpoint
{
    public async Task UploadImage(
        IImageRepository imageRepo,
        Guid siteId,
        IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await imageRepo.SaveSiteImage(siteId, stream, file.FileName);

        if (result.Success)
        {
            return Results.Ok(new { path = result.Path });
        }
        return Results.BadRequest(result.ErrorMessage);
    }
}
```

## Configuration Example

```json
{
    "ImageStorage": {
        "MaxFileSizeBytes": 1048576,
        "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"],
        "AllowedMimeTypes": ["image/jpeg", "image/png", "image/webp"],
        "UploadDirectory": "assets/uploads"
    }
}
```

## Security Considerations

This implementation includes multiple security layers:

-   File extension whitelist validation
-   Magic byte validation (prevents extension spoofing)
-   File size limits
-   Path traversal protection via path normalization checks
-   Separate storage for public vs. protected images

**Note**: Some protections may be intentionally weakened or bypassed in future iterations to demonstrate file upload vulnerabilities for security training purposes.

## Further Reading

-   [OWASP: Unrestricted File Upload](https://owasp.org/www-community/vulnerabilities/Unrestricted_File_Upload) - Common vulnerabilities and mitigations
-   [File Signatures (Magic Numbers)](https://en.wikipedia.org/wiki/List_of_file_signatures) - Understanding magic byte validation
-   [ASP.NET Core Static Files](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files) - Serving uploaded files securely
-   [Content-Type vs File Extension](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types) - Why MIME type validation matters
