using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class DeliveryProofEndpointsTests
{
    private static DeliveryProof CreateProof(Guid? deliveryId = null)
    {
        return new DeliveryProof
        {
            Id = Guid.NewGuid(),
            DeliveryId = deliveryId ?? Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ImagePath = "proofs/test.jpg",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static DeliveryProofDto CreateProofDto(Guid? deliveryId = null)
    {
        return new DeliveryProofDto
        {
            Id = Guid.NewGuid(),
            DeliveryId = deliveryId ?? Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ImagePath = "proofs/test.jpg",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task UploadProofOfDelivery_WithValidFile_ReturnsCreated()
    {
        var deliveryId = Guid.NewGuid();
        var contentBytes = Encoding.UTF8.GetBytes("abc");
        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(contentBytes.Length);
        formFile.Setup(x => x.FileName).Returns("test.jpg");
        formFile.Setup(x => x.OpenReadStream()).Returns(new MemoryStream(contentBytes));

        var proof = CreateProof(deliveryId);
        var proofDto = CreateProofDto(deliveryId);

        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.UploadProofOfDelivery(deliveryId, It.IsAny<ReadOnlyMemory<byte>>(), "test.jpg"))
            .ReturnsAsync(proof);

        var mockMapper = new Mock<IMapper<DeliveryProof, DeliveryProofDto>>();
        mockMapper.Setup(x => x.Map(proof)).Returns(proofDto);

        var result = await DeliveryProofEndpoints.UploadProofOfDelivery(
            mockService.Object,
            mockMapper.Object,
            deliveryId,
            formFile.Object);

        var created = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<DeliveryProofDto>>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(proofDto, created.Value);
    }

    [Fact]
    public async Task UploadProofOfDelivery_WithNullFile_ReturnsBadRequest()
    {
        var deliveryId = Guid.NewGuid();
        var mockService = new Mock<IDeliveryProofService>();
        var mockMapper = new Mock<IMapper<DeliveryProof, DeliveryProofDto>>();

        IFormFile? nullFile = null;

        var result = await DeliveryProofEndpoints.UploadProofOfDelivery(
            mockService.Object,
            mockMapper.Object,
            deliveryId,
            nullFile!);

        var bad = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>(result.Result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task UploadProofOfDelivery_WithEmptyFile_ReturnsBadRequest()
    {
        var deliveryId = Guid.NewGuid();
        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(0);
        formFile.Setup(x => x.FileName).Returns("empty.jpg");

        var mockService = new Mock<IDeliveryProofService>();
        var mockMapper = new Mock<IMapper<DeliveryProof, DeliveryProofDto>>();

        var result = await DeliveryProofEndpoints.UploadProofOfDelivery(
            mockService.Object,
            mockMapper.Object,
            deliveryId,
            formFile.Object);

        var bad = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>(result.Result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task GetProofByDelivery_WhenFound_ReturnsOkWithDto()
    {
        var deliveryId = Guid.NewGuid();
        var proof = CreateProof(deliveryId);
        var proofDto = CreateProofDto(deliveryId);

        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync(proof);

        var mockMapper = new Mock<IMapper<DeliveryProof, DeliveryProofDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<DeliveryProof>())).Returns(proofDto);

        var result = await DeliveryProofEndpoints.GetProofByDelivery(
            mockService.Object,
            mockMapper.Object,
            deliveryId);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(proofDto, result.Value);
    }

    [Fact]
    public async Task GetProofByDelivery_WhenNotFound_ReturnsOkWithNull()
    {
        var deliveryId = Guid.NewGuid();
        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync((DeliveryProof?)null);

        var mockMapper = new Mock<IMapper<DeliveryProof, DeliveryProofDto>>();

        var result = await DeliveryProofEndpoints.GetProofByDelivery(
            mockService.Object,
            mockMapper.Object,
            deliveryId);

        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetProofImage_WhenProofExistsAndFileExists_ReturnsFileStream()
    {
        var deliveryId = Guid.NewGuid();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "proof-test.jpg");
        await File.WriteAllBytesAsync(tempFile, new byte[] { 1, 2, 3 });

        var proof = CreateProof(deliveryId);
        proof.ImagePath = "relative/path.jpg";

        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync(proof);

        var mockRepo = new Mock<IImageRepository>();
        mockRepo.Setup(x => x.GetPhysicalPath(proof.ImagePath)).Returns(tempFile);

        var result = await DeliveryProofEndpoints.GetProofImage(
            mockService.Object,
            mockRepo.Object,
            NullLogger<Program>.Instance,
            deliveryId);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.FileStreamHttpResult>(result.Result);

        try { File.Delete(tempFile); Directory.Delete(tempDir); } catch { }
    }

    [Fact]
    public async Task GetProofImage_WhenProofNotFound_ReturnsNotFound()
    {
        var deliveryId = Guid.NewGuid();
        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync((DeliveryProof?)null);

        var mockRepo = new Mock<IImageRepository>();

        var result = await DeliveryProofEndpoints.GetProofImage(
            mockService.Object,
            mockRepo.Object,
            NullLogger<Program>.Instance,
            deliveryId);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
    }

    [Fact]
    public async Task GetProofImage_WhenPathInvalid_ReturnsNotFound()
    {
        var deliveryId = Guid.NewGuid();
        var proof = CreateProof(deliveryId);

        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync(proof);

        var mockRepo = new Mock<IImageRepository>();
        mockRepo.Setup(x => x.GetPhysicalPath(proof.ImagePath)).Throws(new InvalidOperationException("invalid"));

        var result = await DeliveryProofEndpoints.GetProofImage(
            mockService.Object,
            mockRepo.Object,
            NullLogger<Program>.Instance,
            deliveryId);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
    }

    [Fact]
    public async Task GetProofImage_WhenFileNotOnDisk_ReturnsNotFound()
    {
        var deliveryId = Guid.NewGuid();
        var proof = CreateProof(deliveryId);

        var mockService = new Mock<IDeliveryProofService>();
        mockService.Setup(x => x.GetProofByDeliveryId(deliveryId)).ReturnsAsync(proof);

        var mockRepo = new Mock<IImageRepository>();
        mockRepo.Setup(x => x.GetPhysicalPath(proof.ImagePath)).Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.jpg"));

        var result = await DeliveryProofEndpoints.GetProofImage(
            mockService.Object,
            mockRepo.Object,
            NullLogger<Program>.Instance,
            deliveryId);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
    }
}


