using System;
using MBC.Core.ValueObjects;

namespace MBC.Endpoints.Dtos;

public sealed record DeliveryReviewDto
{
    public required Guid Id { get; init; }
    public required Guid PilotId { get; init; }
    public required Rating Rating { get; init; }
    public required string Notes { get; init; }
    public required DateTime CreatedAt { get; init; }
}


