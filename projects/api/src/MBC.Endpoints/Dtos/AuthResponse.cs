namespace MBC.Endpoints.Dtos;

public sealed record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required UserDto User { get; init; }
}

