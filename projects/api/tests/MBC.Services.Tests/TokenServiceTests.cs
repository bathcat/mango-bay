using System;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Services.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MBC.Services.Tests;

public class TokenServiceTests
{
    private static IOptions<JwtSettings> CreateJwtSettings() =>
        Options.Create(new JwtSettings
        {
            Secret = new string('a', 64),
            Issuer = "issuer",
            Audience = "audience",
            AccessTokenExpirationMinutes = 5,
            RefreshTokenExpirationDays = 14
        });

    [Fact]
    public void GenerateAccessToken_ReturnsToken_WithAllClaims()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "test@email.com" };
        var role = "Admin";
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);

        var jwt = service.GenerateAccessToken(user, role, customerId, pilotId);

        var handler = new JsonWebTokenHandler();
        var token = handler.ReadJsonWebToken(jwt);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == role);
        Assert.Contains(token.Claims, c => c.Type == MBCClaims.CustomerId && c.Value == customerId.ToString());
        Assert.Contains(token.Claims, c => c.Type == MBCClaims.PilotId && c.Value == pilotId.ToString());
        Assert.Equal("issuer", token.Issuer);
        Assert.Contains("audience", token.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsToken_WithMinimalClaims()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "foo@bar.com" };
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);

        var jwt = service.GenerateAccessToken(user, "User", null, null);

        var handler = new JsonWebTokenHandler();
        var token = handler.ReadJsonWebToken(jwt);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        Assert.DoesNotContain(token.Claims, c => c.Type == MBCClaims.CustomerId);
        Assert.DoesNotContain(token.Claims, c => c.Type == MBCClaims.PilotId);
    }

    [Fact]
    public void GenerateOpaqueRefreshToken_GeneratesUrlSafeAndUnique()
    {
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);

        var (token1, hash1) = service.GenerateOpaqueRefreshToken();
        var (token2, hash2) = service.GenerateOpaqueRefreshToken();

        Assert.NotEqual(token1, token2);
        Assert.All(new[] { token1, token2 }, t =>
        {
            Assert.DoesNotContain('+', t);
            Assert.DoesNotContain('/', t);
            Assert.DoesNotContain('=', t);
            Assert.True(t.Length >= 43);
        });
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashToken_ProducesExpectedHash_ForKnownInput()
    {
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);
        var token = "test-token-value";
        var hash1 = service.HashToken(token);
        var hash2 = service.HashToken(token);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);
    }

}
