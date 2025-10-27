using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Models;
using MBC.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBC.Endpoints.Images;

public class FileSystemImageRepository : IImageRepository
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileSystemImageRepository> _logger;
    private readonly ImageStorageOptions _options;
    private readonly string _uploadRoot;
    private readonly string _protectedRoot;

    public FileSystemImageRepository(
        IWebHostEnvironment environment,
        IOptions<ImageStorageOptions> options,
        ILogger<FileSystemImageRepository> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
        _uploadRoot = Path.Combine(_environment.ContentRootPath, _options.UploadDirectory);
        _protectedRoot = Path.Combine(_environment.ContentRootPath, "assets", "protected");
    }

    public Task<ImageUploadResult> SaveProofOfDeliveryImage(
        Guid deliveryId,
        Stream imageStream,
        string originalFileName)
    {
        return SaveImageCore(deliveryId, imageStream, originalFileName, "deliveries", _protectedRoot);
    }

    public Task<ImageUploadResult> SaveSiteImage(
        Guid siteId,
        Stream imageStream,
        string originalFileName)
    {
        return SaveImageCore(siteId, imageStream, originalFileName, "sites", _uploadRoot);
    }

    private async Task<ImageUploadResult> SaveImageCore(
        Guid entityId,
        Stream imageStream,
        string originalFileName,
        string category,
        string storageRoot)
    {
        try
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                return ImageUploadResult.FailureResult("Image stream is empty");
            }

            if (imageStream.Length > _options.MaxFileSizeBytes)
            {
                _logger.LogWarning(
                    "Image upload rejected: file size {FileSize} exceeds limit {MaxSize}",
                    imageStream.Length,
                    _options.MaxFileSizeBytes);
                return ImageUploadResult.FailureResult($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes} bytes");
            }

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning(
                    "Image upload rejected: extension {Extension} not allowed",
                    extension);
                return ImageUploadResult.FailureResult($"File extension {extension} is not allowed");
            }

            var mimeType = await MimeTypes.GetMimeType(imageStream);
            if (mimeType == MimeType.UnsupportedOrUnknown)
            {
                _logger.LogWarning("Image upload rejected: invalid or unsupported image format");
                return ImageUploadResult.FailureResult("File does not appear to be a valid image");
            }

            var fileName = $"{entityId}{extension}";
            var categoryDirectory = Path.Combine(storageRoot, category);

            if (!Directory.Exists(categoryDirectory))
            {
                throw new InvalidOperationException(
                    $"Image storage directory does not exist: {categoryDirectory}. " +
                    "Please create this directory before running the application.");
            }

            var physicalPath = Path.Combine(categoryDirectory, fileName);

            var normalizedPath = Path.GetFullPath(physicalPath);
            var normalizedRoot = Path.GetFullPath(storageRoot);
            if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "Path traversal attempt detected. Normalized path {NormalizedPath} is outside root {Root}",
                    normalizedPath,
                    normalizedRoot);
                return ImageUploadResult.FailureResult("Invalid file path");
            }

            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("Deleted existing image file: {FilePath}", physicalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete existing file: {FilePath}", physicalPath);
                }
            }

            imageStream.Position = 0;
            using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            var relativePath = $"{category}/{fileName}";

            _logger.LogInformation(
                "Successfully saved image for {Category} {EntityId}: {RelativePath}",
                category,
                entityId,
                relativePath);

            return ImageUploadResult.SuccessResult(relativePath, imageStream.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image for {Category} {EntityId}", category, entityId);
            return ImageUploadResult.FailureResult($"An error occurred while saving the image: {ex.Message}");
        }
    }

    public Task<bool> DeleteImage(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("Deleted image: {RelativePath}", relativePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {RelativePath}", relativePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ImageExists(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);
            return Task.FromResult(File.Exists(physicalPath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GetPhysicalPath(string relativePath)
    {
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

        var storageRoot = relativePath.StartsWith("deliveries/", StringComparison.OrdinalIgnoreCase)
            ? _protectedRoot
            : _uploadRoot;

        var physicalPath = Path.Combine(storageRoot, normalizedRelativePath);
        var normalizedPath = Path.GetFullPath(physicalPath);
        var normalizedRoot = Path.GetFullPath(storageRoot);

        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal detected");
        }

        return normalizedPath;
    }
}

