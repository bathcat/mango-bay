using System;
using MBC.Core.Entities;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class DeliveryProofMapper : IMapper<DeliveryProof, DeliveryProofDto>
{
    public DeliveryProofMapper()
    {
    }

    public DeliveryProofDto Map(DeliveryProof source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new DeliveryProofDto
        {
            Id = source.Id,
            DeliveryId = source.DeliveryId,
            PilotId = source.PilotId,
            CustomerId = source.CustomerId,
            ImagePath = source.ImagePath,
            CreatedAt = source.CreatedAt
        };
    }
}

