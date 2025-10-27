namespace MBC.Endpoints.Dtos;

public sealed record SignUpRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Nickname { get; init; }
}

