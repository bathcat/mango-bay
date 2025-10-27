using System;

namespace MBC.Core.Services;



public sealed record AuthResult
{
    public required bool Success { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public string? Nickname { get; init; }
    public string? Role { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? PilotId { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    public static AuthResult SuccessResult(
        string accessToken,
        string refreshToken,
        Guid userId,
        string? email,
        string? nickname,
        string role,
        Guid? customerId,
        Guid? pilotId)
    {
        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            Email = email,
            Nickname = nickname,
            Role = role,
            CustomerId = customerId,
            PilotId = pilotId
        };
    }
}

