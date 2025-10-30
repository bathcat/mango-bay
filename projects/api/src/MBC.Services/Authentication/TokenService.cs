using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MBC.Services.Authentication;

public class TokenService : ITokenService
{
    private const int ClockSkewMinutes = 5;
    private const int RefreshTokenBytes = 32;
    private readonly JwtSettings _jwtSettings;
    private readonly JsonWebTokenHandler _tokenHandler;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JsonWebTokenHandler();
        _logger = logger;
    }

    public string GenerateAccessToken(MBCUser user, string role, Guid? customerId, Guid? pilotId)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtHeaderParameterNames.Kid, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        ];

        if (customerId.HasValue)
        {
            claims.Add(new Claim(MBCClaims.CustomerId, customerId.Value.ToString()));
        }

        if (pilotId.HasValue)
        {
            claims.Add(new Claim(MBCClaims.PilotId, pilotId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        return _tokenHandler.CreateToken(descriptor);
    }

    public (string token, string hash) GenerateOpaqueRefreshToken()
    {
        byte[] tokenBytes = new byte[RefreshTokenBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var hash = HashToken(token);

        return (token, hash);
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }


}

