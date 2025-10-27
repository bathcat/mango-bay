using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Services.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MBC.Services.Tests;

public class SiteServiceTests
{
    private static SiteService CreateService(
        ISiteStore? siteStore = null,
        IImageRepository? imageRepository = null)
    {
        return new SiteService(
            siteStore ?? Mock.Of<ISiteStore>(),
            imageRepository ?? Mock.Of<IImageRepository>(),
            NullLogger<SiteService>.Instance);
    }

    private static CreateOrUpdateSiteRequest CreateValidRequest() => new()
    {
        Name = "Test Site",
        Notes = "Test Notes",
        Island = "Test Island",
        Address = "123 Test Street",
        Location = new Location { X = 10, Y = 20 },
        Status = SiteStatus.Current
    };

    [Fact]
    public async Task CreateSite_ValidRequest_CreatesAndReturnsSite()
    {
        var request = CreateValidRequest();
        var createdSite = new Site
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Notes = request.Notes,
            Island = request.Island,
            Address = request.Address,
            Location = request.Location,
            Status = request.Status
        };

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.Create(It.IsAny<Site>()))
            .ReturnsAsync((Site s) => s);

        var service = CreateService(siteStore: mockStore.Object);

        var result = await service.CreateSite(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Notes, result.Notes);
        Assert.Equal(request.Island, result.Island);
        Assert.Equal(request.Address, result.Address);
        Assert.Equal(request.Location.X, result.Location.X);
        Assert.Equal(request.Location.Y, result.Location.Y);
        Assert.Equal(request.Status, result.Status);
        mockStore.Verify(x => x.Create(It.IsAny<Site>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSite_ValidRequest_UpdatesAndReturnsSite()
    {
        var siteId = Guid.NewGuid();
        var existingSite = new Site
        {
            Id = siteId,
            Name = "Old Name",
            Notes = "Old Notes",
            Island = "Old Island",
            Address = "Old Address",
            Location = new Location { X = 1, Y = 2 },
            Status = SiteStatus.Upcoming
        };

        var request = CreateValidRequest();

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(existingSite);
        mockStore.Setup(x => x.Update(It.IsAny<Site>()))
            .ReturnsAsync((Site s) => s);

        var service = CreateService(siteStore: mockStore.Object);

        var result = await service.UpdateSite(siteId, request);

        Assert.Equal(siteId, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Notes, result.Notes);
        Assert.Equal(request.Island, result.Island);
        Assert.Equal(request.Address, result.Address);
        Assert.Equal(request.Location.X, result.Location.X);
        Assert.Equal(request.Location.Y, result.Location.Y);
        Assert.Equal(request.Status, result.Status);
        mockStore.Verify(x => x.Update(It.IsAny<Site>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSite_SiteNotFound_ThrowsInvalidOperationException()
    {
        var siteId = Guid.NewGuid();
        var request = CreateValidRequest();

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync((Site?)null);

        var service = CreateService(siteStore: mockStore.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateSite(siteId, request));

        Assert.Contains(siteId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteSite_SiteWithDeliveries_SetsStatusToInactive()
    {
        var siteId = Guid.NewGuid();
        var existingSite = new Site
        {
            Id = siteId,
            Name = "Test Site",
            Status = SiteStatus.Current
        };

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(existingSite);
        mockStore.Setup(x => x.HasAssociatedDeliveries(siteId)).ReturnsAsync(true);
        mockStore.Setup(x => x.Update(It.IsAny<Site>()))
            .ReturnsAsync((Site s) => s);

        var service = CreateService(siteStore: mockStore.Object);

        await service.DeleteSite(siteId);

        mockStore.Verify(x => x.HasAssociatedDeliveries(siteId), Times.Once);
        mockStore.Verify(x => x.Update(It.Is<Site>(s => s.Status == SiteStatus.Inactive)), Times.Once);
        mockStore.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSite_SiteWithoutDeliveries_HardDeletesSite()
    {
        var siteId = Guid.NewGuid();
        var existingSite = new Site
        {
            Id = siteId,
            Name = "Test Site",
            Status = SiteStatus.Current
        };

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(existingSite);
        mockStore.Setup(x => x.HasAssociatedDeliveries(siteId)).ReturnsAsync(false);
        mockStore.Setup(x => x.Delete(siteId)).Returns(Task.CompletedTask);

        var service = CreateService(siteStore: mockStore.Object);

        await service.DeleteSite(siteId);

        mockStore.Verify(x => x.HasAssociatedDeliveries(siteId), Times.Once);
        mockStore.Verify(x => x.Delete(siteId), Times.Once);
        mockStore.Verify(x => x.Update(It.IsAny<Site>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSite_SiteNotFound_ThrowsInvalidOperationException()
    {
        var siteId = Guid.NewGuid();

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync((Site?)null);

        var service = CreateService(siteStore: mockStore.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteSite(siteId));

        Assert.Contains(siteId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetSiteById_ExistingSite_ReturnsSite()
    {
        var siteId = Guid.NewGuid();
        var site = new Site { Id = siteId, Name = "Test Site" };

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(site);

        var service = CreateService(siteStore: mockStore.Object);

        var result = await service.GetSiteById(siteId);

        Assert.NotNull(result);
        Assert.Equal(siteId, result!.Id);
    }

    [Fact]
    public async Task GetSiteById_NonExistingSite_ReturnsNull()
    {
        var siteId = Guid.NewGuid();

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync((Site?)null);

        var service = CreateService(siteStore: mockStore.Object);

        var result = await service.GetSiteById(siteId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UploadSiteImage_ValidSite_UploadsAndUpdatesImageUrl()
    {
        var siteId = Guid.NewGuid();
        var existingSite = new Site { Id = siteId, Name = "Test Site" };
        var imageStream = new MemoryStream();
        var fileName = "test.jpg";
        var relativePath = "sites/123/image.jpg";

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(existingSite);
        mockStore.Setup(x => x.Update(It.IsAny<Site>()))
            .ReturnsAsync((Site s) => s);

        var mockImageRepo = new Mock<IImageRepository>();
        mockImageRepo.Setup(x => x.SaveSiteImage(siteId, imageStream, fileName))
            .ReturnsAsync(ImageUploadResult.SuccessResult(relativePath, 1024));

        var service = CreateService(
            siteStore: mockStore.Object,
            imageRepository: mockImageRepo.Object);

        var result = await service.UploadSiteImage(siteId, imageStream, fileName);

        Assert.True(result.Success);
        Assert.Equal(relativePath, result.RelativePath);
        Assert.Equal(relativePath, existingSite.ImageUrl);
        mockImageRepo.Verify(x => x.SaveSiteImage(siteId, imageStream, fileName), Times.Once);
        mockStore.Verify(x => x.Update(existingSite), Times.Once);
    }

    [Fact]
    public async Task UploadSiteImage_SiteNotFound_ReturnsFailureResult()
    {
        var siteId = Guid.NewGuid();
        var imageStream = new MemoryStream();
        var fileName = "test.jpg";

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync((Site?)null);

        var service = CreateService(siteStore: mockStore.Object);

        var result = await service.UploadSiteImage(siteId, imageStream, fileName);

        Assert.False(result.Success);
        Assert.Contains(siteId.ToString(), result.ErrorMessage);
    }

    [Fact]
    public async Task UploadSiteImage_UploadFails_DoesNotUpdateSite()
    {
        var siteId = Guid.NewGuid();
        var existingSite = new Site { Id = siteId, Name = "Test Site" };
        var imageStream = new MemoryStream();
        var fileName = "test.jpg";

        var mockStore = new Mock<ISiteStore>();
        mockStore.Setup(x => x.GetById(siteId)).ReturnsAsync(existingSite);

        var mockImageRepo = new Mock<IImageRepository>();
        mockImageRepo.Setup(x => x.SaveSiteImage(siteId, imageStream, fileName))
            .ReturnsAsync(ImageUploadResult.FailureResult("Upload failed"));

        var service = CreateService(
            siteStore: mockStore.Object,
            imageRepository: mockImageRepo.Object);

        var result = await service.UploadSiteImage(siteId, imageStream, fileName);

        Assert.False(result.Success);
        Assert.Null(existingSite.ImageUrl);
        mockStore.Verify(x => x.Update(It.IsAny<Site>()), Times.Never);
    }
}

