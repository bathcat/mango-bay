using System;
using MBC.Core.Entities;

namespace MBC.Endpoints.Dtos;

public sealed record SiteDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Notes { get; init; }
    public required string Island { get; init; }
    public required string Address { get; init; }
    public required Location Location { get; init; }
    public required SiteStatus Status { get; init; }
    public string? ImageUrl { get; init; }
}

