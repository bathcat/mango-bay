using System;

namespace MBC.Endpoints.Dtos;

public sealed record DeliveryProofDto
{
    public required Guid Id { get; init; }
    public required Guid DeliveryId { get; init; }
    public required Guid PilotId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string ImagePath { get; init; }
    public required DateTime CreatedAt { get; init; }
}

