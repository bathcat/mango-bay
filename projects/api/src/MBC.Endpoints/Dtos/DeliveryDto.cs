using System;
using MBC.Core.Entities;

namespace MBC.Endpoints.Dtos;

public sealed record DeliveryDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid PilotId { get; init; }
    public required Guid OriginId { get; init; }
    public required Guid DestinationId { get; init; }
    public required DateOnly ScheduledFor { get; init; }
    public DateTime? CompletedOn { get; init; }
    public required DeliveryStatus Status { get; init; }
    public required string CargoDescription { get; init; }
    public required decimal CargoWeightKg { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required Guid PaymentId { get; init; }
}

