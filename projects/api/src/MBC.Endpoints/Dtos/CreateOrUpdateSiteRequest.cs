using MBC.Core.Entities;

namespace MBC.Endpoints.Dtos;

public sealed record CreateOrUpdateSiteRequest
{
    public required string Name { get; init; }
    public required string Notes { get; init; }
    public required string Island { get; init; }
    public required string Address { get; init; }
    public required Location Location { get; init; }
    public required SiteStatus Status { get; init; }
}

