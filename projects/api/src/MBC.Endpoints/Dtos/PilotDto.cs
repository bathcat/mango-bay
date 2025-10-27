using System;

namespace MBC.Endpoints.Dtos;

public sealed record PilotDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string ShortName { get; init; }
    public string? AvatarUrl { get; init; }
    public required string Bio { get; init; }
}

