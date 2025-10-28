using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using MBC.Endpoints.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class FileSystemImageRepositoryTests
{
    [Fact]
    public async Task SaveSiteImage_WithDirectoryTraversalInOriginalFilename_UsesGuidFilenameAndDoesNotEscape()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var maliciousFilename = "../../../../../../etc/passwd.jpg";

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, maliciousFilename);

        Assert.True(result.Success);
        Assert.Equal($"sites/{siteId}.jpg", result.RelativePath);
        Assert.True(fileSystem.File.Exists($"{sitesDir}/{siteId}.jpg"));
        Assert.False(fileSystem.File.Exists("/etc/passwd.jpg"));
    }


    [Fact]
    public async Task SaveSiteImage_WithAbsoluteWindowsPathInFilename_DoesNotEscapeRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "C:\\app";
        var sitesDir = "C:\\app\\assets\\uploads\\sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets\\uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var maliciousFilename = "C:\\Windows\\System32\\evil.jpg";

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, maliciousFilename);

        Assert.True(result.Success);
        Assert.True(fileSystem.File.Exists($"{sitesDir}\\{siteId}.jpg"));
        Assert.False(fileSystem.File.Exists("C:\\Windows\\System32\\evil.jpg"));
    }

    [Fact]
    public async Task SaveSiteImage_WithAbsoluteUnixPathInFilename_DoesNotEscapeRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var maliciousFilename = "/etc/passwd.jpg";

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, maliciousFilename);

        Assert.True(result.Success);
        Assert.True(fileSystem.File.Exists($"{sitesDir}/{siteId}.jpg"));
        Assert.False(fileSystem.File.Exists("/etc/passwd.jpg"));
    }

    [Fact]
    public void GetPhysicalPath_WithDirectoryTraversal_ThrowsException()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        fileSystem.AddDirectory("/app/assets/uploads/sites");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            repository.GetPhysicalPath("sites/../../secrets/config.json"));

        Assert.Equal("Path traversal detected", exception.Message);
    }

    [Fact]
    public void GetPhysicalPath_WithMixedSeparators_DoesNotEscapeRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        fileSystem.AddDirectory("/app/assets/uploads/sites");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            repository.GetPhysicalPath("sites/..\\../secrets/config.json"));

        Assert.Equal("Path traversal detected", exception.Message);
    }

    [Fact]
    public async Task SaveSiteImage_WithOversizeFile_ReturnsFailure()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var maxSize = 1000;
        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = maxSize,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var oversizedData = new byte[maxSize + 1];
        Array.Fill<byte>(oversizedData, 0xFF);

        var result = await repository.SaveSiteImage(siteId, oversizedData, "photo.jpg");

        Assert.False(result.Success);
        Assert.Contains("exceeds maximum allowed size", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveSiteImage_WithDisallowedExtension_ReturnsFailure()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, "malware.exe");

        Assert.False(result.Success);
        Assert.Contains(".exe", result.ErrorMessage);
        Assert.Contains("not allowed", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveSiteImage_WithNoExtension_ReturnsFailure()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, "noextension");

        Assert.False(result.Success);
        Assert.Contains("not valid", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveSiteImage_WithMismatchedMimeType_ReturnsFailure()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var textData = Encoding.UTF8.GetBytes("This is not an image file");

        var result = await repository.SaveSiteImage(siteId, textData, "fake.jpg");

        Assert.False(result.Success);
        Assert.Contains("not appear to be a valid image", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveSiteImage_WithEmptyData_ReturnsFailure()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var emptyData = ReadOnlyMemory<byte>.Empty;

        var result = await repository.SaveSiteImage(siteId, emptyData, "photo.jpg");

        Assert.False(result.Success);
        Assert.Equal("Image data is empty", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveSiteImage_WithValidJpegData_ReturnsSuccess()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, "photo.jpg");

        Assert.True(result.Success);
        Assert.Equal($"sites/{siteId}.jpg", result.RelativePath);
        Assert.Equal(jpegMagicBytes.Length, result.FileSize);
        Assert.True(fileSystem.File.Exists($"{sitesDir}/{siteId}.jpg"));
    }

    [Fact]
    public async Task SaveProofOfDeliveryImage_WithValidPngData_ReturnsSuccess()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var deliveriesDir = "/app/assets/protected/deliveries";
        fileSystem.AddDirectory(deliveriesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var deliveryId = Guid.NewGuid();
        var pngMagicBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var result = await repository.SaveProofOfDeliveryImage(deliveryId, pngMagicBytes, "proof.png");

        Assert.True(result.Success);
        Assert.Equal($"deliveries/{deliveryId}.png", result.RelativePath);
        Assert.Equal(pngMagicBytes.Length, result.FileSize);
        Assert.True(fileSystem.File.Exists($"{deliveriesDir}/{deliveryId}.png"));
    }

    [Fact]
    public async Task SaveSiteImage_WhenDirectoryDoesNotExist_ThrowsException()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        var result = await repository.SaveSiteImage(siteId, jpegMagicBytes, "photo.jpg");

        Assert.False(result.Success);
        Assert.Contains("error occurred", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveSiteImage_ReplacingExistingFile_DeletesAndRecreates()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var jpegMagicBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        await repository.SaveSiteImage(siteId, jpegMagicBytes, "photo1.jpg");

        var newJpegData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x20, 0x45, 0x78, 0x69, 0x66 };
        var result = await repository.SaveSiteImage(siteId, newJpegData, "photo2.jpg");

        Assert.True(result.Success);
        Assert.Equal(newJpegData.Length, result.FileSize);
        var savedData = fileSystem.File.ReadAllBytes($"{sitesDir}/{siteId}.jpg");
        Assert.Equal(newJpegData, savedData);
    }

    [Fact]
    public async Task DeleteImage_WithExistingFile_ReturnsTrue()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        var fileName = "test-image.jpg";
        var filePath = $"{sitesDir}/{fileName}";
        fileSystem.AddDirectory(sitesDir);
        fileSystem.AddFile(filePath, new MockFileData("fake image data"));

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var result = await repository.DeleteImage($"sites/{fileName}");

        Assert.True(result);
        Assert.False(fileSystem.File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteImage_WithNonExistentFile_ReturnsFalse()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var result = await repository.DeleteImage("sites/nonexistent.jpg");

        Assert.False(result);
    }

    [Fact]
    public async Task ImageExists_WithExistingFile_ReturnsTrue()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        var fileName = "existing-image.jpg";
        var filePath = $"{sitesDir}/{fileName}";
        fileSystem.AddDirectory(sitesDir);
        fileSystem.AddFile(filePath, new MockFileData("fake image data"));

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var result = await repository.ImageExists($"sites/{fileName}");

        Assert.True(result);
    }

    [Fact]
    public async Task ImageExists_WithNonExistentFile_ReturnsFalse()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        var sitesDir = "/app/assets/uploads/sites";
        fileSystem.AddDirectory(sitesDir);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var result = await repository.ImageExists("sites/missing.jpg");

        Assert.False(result);
    }

    [Fact]
    public void GetPhysicalPath_WithDeliveryPath_UsesProtectedRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        fileSystem.AddDirectory("/app/assets/uploads");
        fileSystem.AddDirectory("/app/assets/protected/deliveries");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var deliveryId = Guid.NewGuid();
        var path = repository.GetPhysicalPath($"deliveries/{deliveryId}.jpg");

        Assert.Contains("protected", path);
        Assert.DoesNotContain("uploads", path);
    }

    [Fact]
    public void GetPhysicalPath_WithSitePath_UsesUploadRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        fileSystem.AddDirectory("/app/assets/uploads/sites");
        fileSystem.AddDirectory("/app/assets/protected");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var siteId = Guid.NewGuid();
        var path = repository.GetPhysicalPath($"sites/{siteId}.jpg");

        Assert.Contains("uploads", path);
        Assert.DoesNotContain("protected", path);
    }

    [Fact]
    public void GetPhysicalPath_WithCraftedDeliveryPrefix_CannotAccessUploadRoot()
    {
        var fileSystem = new MockFileSystem();
        var contentRoot = "/app";
        fileSystem.AddDirectory("/app/assets/uploads");
        fileSystem.AddDirectory("/app/assets/protected");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(contentRoot);

        var options = Options.Create(new ImageStorageOptions
        {
            UploadDirectory = "assets/uploads",
            MaxFileSizeBytes = 1_048_576,
            AllowedExtensions = [
                Images.FileExtension.From(".jpg"),
                Images.FileExtension.From(".jpeg"),
                Images.FileExtension.From(".png"),
                Images.FileExtension.From(".webp")
            ],
            AllowedMimeTypes = [
                Images.MimeTypeValue.ImageJpeg,
                Images.MimeTypeValue.ImagePng,
                Images.MimeTypeValue.ImageWebP
            ]
        });

        var repository = new FileSystemImageRepository(
            environment.Object,
            options,
            NullLogger<FileSystemImageRepository>.Instance,
            fileSystem.File,
            fileSystem.Directory,
            fileSystem.FileStream,
            fileSystem.Path);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            repository.GetPhysicalPath("deliveries/../../uploads/sites/stolen.jpg"));

        Assert.Equal("Path traversal detected", exception.Message);
    }
}

