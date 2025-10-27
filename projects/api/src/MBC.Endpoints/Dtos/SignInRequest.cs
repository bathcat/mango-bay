namespace MBC.Endpoints.Dtos;

public sealed record SignInRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

