using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

public interface IDeliveryProofStore : IStore<Guid, DeliveryProof>
{
    Task<DeliveryProof> Create(DeliveryProof proof);
    Task<DeliveryProof?> GetByDeliveryId(Guid deliveryId);
    Task<bool> Delete(Guid id);
}

