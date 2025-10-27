namespace MBC.Endpoints.Dtos;

public sealed record UpdateCustomerRequest
{
    public required string Nickname { get; init; }
}

