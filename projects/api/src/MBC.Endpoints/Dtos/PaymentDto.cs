using System;
using MBC.Core.Entities;

namespace MBC.Endpoints.Dtos;

public sealed record PaymentDto
{
    public required Guid Id { get; init; }
    public required Guid DeliveryId { get; init; }
    public required decimal Amount { get; init; }
    public required PaymentStatus Status { get; init; }
    public required string TransactionId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

