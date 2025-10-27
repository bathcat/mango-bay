using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Services;

public interface IDeliveryProofService
{
    Task<DeliveryProof> UploadProofOfDelivery(Guid deliveryId, Stream imageStream, string fileName);
    Task<DeliveryProof?> GetProofByDeliveryId(Guid deliveryId);
    Task<DeliveryProof?> GetProofById(Guid id);
}

