using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;

namespace MBC.Core.Services;

public interface ISiteService
{
    Task<Site> CreateSite(CreateOrUpdateSiteRequest request);

    Task<Site> UpdateSite(Guid siteId, CreateOrUpdateSiteRequest request);

    Task DeleteSite(Guid siteId);

    Task<Site?> GetSiteById(Guid siteId);

    Task<ImageUploadResult> UploadSiteImage(Guid siteId, ReadOnlyMemory<byte> imageData, string fileName);
}

