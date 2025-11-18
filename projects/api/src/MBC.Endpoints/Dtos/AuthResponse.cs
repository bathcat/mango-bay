namespace MBC.Endpoints.Dtos;

public sealed record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required UserDto User { get; init; }
}

//TODO Bust this into a new file.
public sealed record AuthWebResponse
{
    public required UserDto User { get; init; }
}

