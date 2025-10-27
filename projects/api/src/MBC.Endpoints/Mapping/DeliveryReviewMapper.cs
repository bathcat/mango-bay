using System;
using MBC.Core.Entities;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class DeliveryReviewMapper : IMapper<DeliveryReview, DeliveryReviewDto>
{


    public DeliveryReviewMapper()
    {
    }

    public DeliveryReviewDto Map(DeliveryReview source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new DeliveryReviewDto
        {
            Id = source.Id,
            PilotId = source.PilotId,
            Rating = source.Rating,
            Notes = source.Notes,
            CreatedAt = source.CreatedAt
        };
    }
}


