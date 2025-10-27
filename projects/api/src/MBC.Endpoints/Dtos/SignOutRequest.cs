namespace MBC.Endpoints.Dtos;

public sealed record SignOutRequest
{
    public required string RefreshToken { get; init; }
}


