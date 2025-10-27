using System;
using MBC.Core.ValueObjects;

namespace MBC.Endpoints.Dtos;

public sealed record CreateReviewRequest
{
    public required Guid DeliveryId { get; init; }
    public required Rating Rating { get; init; }
    public required string Notes { get; init; }
}

