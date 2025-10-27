using System;

namespace MBC.Endpoints.Dtos;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? Nickname { get; init; }
    public required string Role { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? PilotId { get; init; }
}

