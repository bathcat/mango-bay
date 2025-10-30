using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class SiteEndpointsTests
{
    private static Site CreateSite() => new Site
    {
        Id = Guid.NewGuid(),
        Name = "Test Site",
        Notes = "Test notes",
        Island = "Test Island",
        Address = "Test Address",
        Location = new Location { X = 10, Y = 20 },
        Status = SiteStatus.Current,
        ImageUrl = null
    };

    private static SiteDto CreateSiteDto() => new SiteDto
    {
        Id = Guid.NewGuid(),
        Name = "Test Site",
        Notes = "Test notes",
        Island = "Test Island",
        Address = "Test Address",
        Location = new Location { X = 10, Y = 20 },
        Status = SiteStatus.Current,
        ImageUrl = null
    };

    private static Dtos.CreateOrUpdateSiteRequest CreateCreateOrUpdateSiteRequest() => new Dtos.CreateOrUpdateSiteRequest
    {
        Name = "Test Site",
        Notes = "Test notes",
        Island = "Test Island",
        Address = "Test Address",
        Location = new Location { X = 10, Y = 20 },
        Status = SiteStatus.Current
    };

    [Fact]
    public async Task GetSites_ReturnsPaginatedSites()
    {
        var skip = 10;
        var take = 20;

        var sites = Page.Create(
            items: [CreateSite(), CreateSite()],
            offset: 0,
            countRequested: 20,
            totalCount: 2
        );

        var mockSiteStore = new Mock<ISiteStore>();
        mockSiteStore.Setup(x => x.GetPage(skip, take)).ReturnsAsync(sites);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Site>())).Returns(CreateSiteDto());

        var result = await SiteEndpoints.GetSites(
            mockSiteStore.Object,
            mockMapper.Object,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        mockSiteStore.Verify(x => x.GetPage(skip, take), Times.Once);
    }

    [Fact]
    public async Task GetSites_UsesDefaultPaginationWhenNotProvided()
    {
        var sites = Page.Create(
            items: [CreateSite()],
            offset: 0,
            countRequested: 10,
            totalCount: 1
        );

        var mockSiteStore = new Mock<ISiteStore>();
        mockSiteStore.Setup(x => x.GetPage(0, 10)).ReturnsAsync(sites);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Site>())).Returns(CreateSiteDto());

        var result = await SiteEndpoints.GetSites(
            mockSiteStore.Object,
            mockMapper.Object);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        mockSiteStore.Verify(x => x.GetPage(0, 10), Times.Once);
    }

    [Fact]
    public async Task GetSite_WhenFound_ReturnsOkWithDto()
    {
        var siteId = Guid.NewGuid();
        var site = CreateSite();
        site.Id = siteId;
        var siteDto = CreateSiteDto();
        siteDto = siteDto with { Id = siteId };

        var mockSiteStore = new Mock<ISiteStore>();
        mockSiteStore.Setup(x => x.GetById(siteId)).ReturnsAsync(site);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();
        mockMapper.Setup(x => x.Map(site)).Returns(siteDto);

        var result = await SiteEndpoints.GetSite(
            mockSiteStore.Object,
            mockMapper.Object,
            siteId);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<SiteDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(siteDto, okResult.Value);
    }

    [Fact]
    public async Task GetSite_WhenNotFound_ReturnsNotFound()
    {
        var siteId = Guid.NewGuid();

        var mockSiteStore = new Mock<ISiteStore>();
        mockSiteStore.Setup(x => x.GetById(siteId)).ReturnsAsync((Site?)null);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();

        var result = await SiteEndpoints.GetSite(
            mockSiteStore.Object,
            mockMapper.Object,
            siteId);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
    }

    [Fact]
    public async Task CreateSite_CreatesAndReturnsSite()
    {
        var request = CreateCreateOrUpdateSiteRequest();
        var site = CreateSite();
        var siteDto = CreateSiteDto();

        var mockSiteService = new Mock<ISiteService>();
        mockSiteService.Setup(x => x.CreateSite(It.IsAny<MBC.Core.Models.CreateOrUpdateSiteRequest>())).ReturnsAsync(site);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();
        mockMapper.Setup(x => x.Map(site)).Returns(siteDto);

        var result = await SiteEndpoints.CreateSite(
            mockSiteService.Object,
            mockMapper.Object,
            (Dtos.CreateOrUpdateSiteRequest)request);

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(siteDto, result.Value);

        mockSiteService.Verify(x => x.CreateSite(It.IsAny<MBC.Core.Models.CreateOrUpdateSiteRequest>()), Times.Once);
        mockMapper.Verify(x => x.Map(site), Times.Once);
    }

    [Fact]
    public async Task UpdateSite_WhenFound_ReturnsOkWithUpdatedDto()
    {
        var siteId = Guid.NewGuid();
        var request = CreateCreateOrUpdateSiteRequest();
        var updatedSite = CreateSite();
        updatedSite.Id = siteId;
        var updatedSiteDto = CreateSiteDto();
        updatedSiteDto = updatedSiteDto with { Id = siteId };

        var mockSiteService = new Mock<ISiteService>();
        mockSiteService.Setup(x => x.UpdateSite(siteId, It.IsAny<MBC.Core.Models.CreateOrUpdateSiteRequest>())).ReturnsAsync(updatedSite);

        var mockMapper = new Mock<IMapper<Site, SiteDto>>();
        mockMapper.Setup(x => x.Map(updatedSite)).Returns(updatedSiteDto);

        var result = await SiteEndpoints.UpdateSite(
            mockSiteService.Object,
            mockMapper.Object,
            siteId,
            (Dtos.CreateOrUpdateSiteRequest)request);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<SiteDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(updatedSiteDto, okResult.Value);
    }

    [Fact]
    public async Task DeleteSite_DeletesSiteSuccessfully()
    {
        var siteId = Guid.NewGuid();

        var mockSiteService = new Mock<ISiteService>();
        mockSiteService.Setup(x => x.DeleteSite(siteId)).Returns(Task.CompletedTask);

        var result = await SiteEndpoints.DeleteSite(
            mockSiteService.Object,
            siteId);

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);

        mockSiteService.Verify(x => x.DeleteSite(siteId), Times.Once);
    }

    [Fact]
    public async Task UploadSiteImage_WhenValidFile_ReturnsOkWithPath()
    {
        var siteId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3, 4 };
        var fileName = "test.jpg";
        var relativePath = "uploads/test.jpg";

        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(imageData.Length);
        formFile.Setup(x => x.FileName).Returns(fileName);
        formFile.Setup(x => x.OpenReadStream()).Returns(new MemoryStream(imageData));

        var uploadResult = ImageUploadResult.SuccessResult(relativePath, imageData.Length);

        var mockSiteService = new Mock<ISiteService>();
        mockSiteService.Setup(x => x.UploadSiteImage(siteId, It.IsAny<ReadOnlyMemory<byte>>(), fileName)).ReturnsAsync(uploadResult);

        var result = await SiteEndpoints.UploadSiteImage(
            mockSiteService.Object,
            siteId,
            formFile.Object);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(relativePath, okResult.Value);
    }


    [Fact]
    public async Task UploadSiteImage_WhenEmptyFile_ReturnsBadRequest()
    {
        var siteId = Guid.NewGuid();

        var emptyFormFile = new Mock<IFormFile>();
        emptyFormFile.Setup(x => x.Length).Returns(0);

        var result = await SiteEndpoints.UploadSiteImage(
            Mock.Of<ISiteService>(),
            siteId,
            emptyFormFile.Object);

        var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("No file uploaded", badRequestResult.Value);
    }

    [Fact]
    public async Task UploadSiteImage_WhenUploadFails_ReturnsBadRequest()
    {
        var siteId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3, 4 };
        var fileName = "test.jpg";
        var errorMessage = "Upload failed due to disk space";

        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(imageData.Length);
        formFile.Setup(x => x.FileName).Returns(fileName);
        formFile.Setup(x => x.OpenReadStream()).Returns(new MemoryStream(imageData));

        var uploadResult = ImageUploadResult.FailureResult(errorMessage);

        var mockSiteService = new Mock<ISiteService>();
        mockSiteService.Setup(x => x.UploadSiteImage(siteId, It.IsAny<ReadOnlyMemory<byte>>(), fileName)).ReturnsAsync(uploadResult);

        var result = await SiteEndpoints.UploadSiteImage(
            mockSiteService.Object,
            siteId,
            formFile.Object);

        var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }
}
