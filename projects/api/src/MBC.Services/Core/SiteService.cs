using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Core;

public class SiteService : ISiteService
{
    private readonly ISiteStore _siteStore;
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<SiteService> _logger;

    public SiteService(ISiteStore siteStore, IImageRepository imageRepository, ILogger<SiteService> logger)
    {
        _siteStore = siteStore;
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public async Task<Site> CreateSite(CreateOrUpdateSiteRequest request)
    {
        _logger.LogInformation("Creating site {Name}", request.Name);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Notes = request.Notes,
            Island = request.Island,
            Address = request.Address,
            Location = request.Location,
            Status = request.Status
        };

        var createdSite = await _siteStore.Create(site);

        _logger.LogInformation("Successfully created site {SiteId}", createdSite.Id);

        return createdSite;
    }

    public async Task<Site> UpdateSite(Guid siteId, CreateOrUpdateSiteRequest request)
    {
        _logger.LogInformation("Updating site {SiteId}", siteId);

        var site = await _siteStore.GetById(siteId);
        if (site == null)
        {
            _logger.LogWarning("Site {SiteId} not found", siteId);
            throw new InvalidOperationException($"Site with ID {siteId} not found.");
        }

        site.Name = request.Name;
        site.Notes = request.Notes;
        site.Island = request.Island;
        site.Address = request.Address;
        site.Location = request.Location;
        site.Status = request.Status;

        var updatedSite = await _siteStore.Update(site);

        _logger.LogInformation("Successfully updated site {SiteId}", siteId);

        return updatedSite;
    }

    public async Task DeleteSite(Guid siteId)
    {
        _logger.LogInformation("Deleting site {SiteId}", siteId);

        var site = await _siteStore.GetById(siteId);
        if (site == null)
        {
            _logger.LogWarning("Site {SiteId} not found", siteId);
            throw new InvalidOperationException($"Site with ID {siteId} not found.");
        }

        var hasAssociatedDeliveries = await _siteStore.HasAssociatedDeliveries(siteId);

        if (hasAssociatedDeliveries)
        {
            _logger.LogInformation("Site {SiteId} has associated deliveries, setting to Inactive", siteId);
            site.Status = SiteStatus.Inactive;
            await _siteStore.Update(site);
        }
        else
        {
            _logger.LogInformation("Site {SiteId} has no associated deliveries, deleting", siteId);
            await _siteStore.Delete(siteId);
        }

        _logger.LogInformation("Successfully deleted/inactivated site {SiteId}", siteId);
    }

    public async Task<Site?> GetSiteById(Guid siteId)
    {
        return await _siteStore.GetById(siteId);
    }

    public async Task<ImageUploadResult> UploadSiteImage(Guid siteId, Stream imageStream, string fileName)
    {
        _logger.LogInformation("Uploading site image for site {SiteId}", siteId);

        var site = await _siteStore.GetById(siteId);
        if (site == null)
        {
            _logger.LogWarning("Site {SiteId} not found", siteId);
            return ImageUploadResult.FailureResult($"Site with ID {siteId} not found.");
        }

        var result = await _imageRepository.SaveSiteImage(siteId, imageStream, fileName);

        if (result.Success && result.RelativePath != null)
        {
            site.ImageUrl = result.RelativePath;
            await _siteStore.Update(site);

            _logger.LogInformation(
                "Successfully uploaded site image for site {SiteId}: {Path}",
                siteId,
                result.RelativePath);
        }

        return result;
    }
}

