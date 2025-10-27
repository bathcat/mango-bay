using MBC.Core.Entities;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class DeliveryMapper : IMapper<Delivery, DeliveryDto>
{
    public DeliveryDto Map(Delivery source)
    {
        return new DeliveryDto
        {
            Id = source.Id,
            CustomerId = source.CustomerId,
            PilotId = source.PilotId,
            OriginId = source.Details.OriginId,
            DestinationId = source.Details.DestinationId,
            ScheduledFor = source.Details.ScheduledFor,
            CompletedOn = source.CompletedOn,
            Status = source.Status,
            CargoDescription = source.Details.CargoDescription,
            CargoWeightKg = source.Details.CargoWeightKg,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            PaymentId = source.PaymentId
        };
    }
}

