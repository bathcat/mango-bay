using System;

namespace MBC.Endpoints.Dtos;

public sealed record CustomerDto
{
    public required Guid Id { get; init; }
    public required string Nickname { get; init; }
}

