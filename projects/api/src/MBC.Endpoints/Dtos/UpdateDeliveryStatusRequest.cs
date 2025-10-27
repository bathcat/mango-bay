using MBC.Core.Entities;

namespace MBC.Endpoints.Dtos;

public sealed record UpdateDeliveryStatusRequest
{
    public required DeliveryStatus Status { get; init; }
}

