using System;
using System.Threading.Tasks;
using MBC.Core.Models;

namespace MBC.Core.Services;

public interface IImageRepository
{
    Task<ImageUploadResult> SaveProofOfDeliveryImage(
        Guid deliveryId,
        ReadOnlyMemory<byte> imageData,
        string originalFileName);

    Task<ImageUploadResult> SaveSiteImage(
        Guid siteId,
        ReadOnlyMemory<byte> imageData,
        string originalFileName);

    Task<bool> DeleteImage(string relativePath);

    Task<bool> ImageExists(string relativePath);

    string GetPhysicalPath(string relativePath);
}

