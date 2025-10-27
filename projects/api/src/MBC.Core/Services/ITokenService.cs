using System;
using System.Security.Claims;
using MBC.Core.Entities;

namespace MBC.Core.Services;

public interface ITokenService
{
    string GenerateAccessToken(MBCUser user, string role, Guid? customerId, Guid? pilotId);

    (string token, string hash) GenerateOpaqueRefreshToken();

    string HashToken(string token);

    ClaimsPrincipal? ValidateAccessToken(string token);
}

