using MBC.Core.ValueObjects;

namespace MBC.Endpoints.Dtos;

public sealed record UpdateReviewRequest
{
    public required Rating Rating { get; init; }
    public required string Notes { get; init; }
}

