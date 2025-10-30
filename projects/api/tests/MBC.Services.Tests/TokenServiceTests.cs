using System;
using System.IdentityModel.Tokens.Jwt;
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

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
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

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
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

    [Fact]
    public void ValidateAccessToken_WithValidToken_Succeeds()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "alice@example.com" };
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);
        var jwt = service.GenerateAccessToken(user, "User", null, null);

        var principal = service.ValidateAccessToken(jwt);

        Assert.NotNull(principal);
        Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        Assert.Contains(principal.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }

    [Fact]
    public void ValidateAccessToken_WithTamperedToken_Fails()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "abc@xyz.com" };
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);
        var jwt = service.GenerateAccessToken(user, "User", null, null);

        var parts = jwt.Split('.');
        var tamperedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"hacker\"}"));
        var tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

        var principal = service.ValidateAccessToken(tamperedToken);

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidFormat_Fails()
    {
        var service = new TokenService(CreateJwtSettings(), NullLogger<TokenService>.Instance);

        var principal = service.ValidateAccessToken("not.a.jwt");

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithWrongIssuer_Fails()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "issuer@fail.com" };
        var settings = Options.Create(new JwtSettings
        {
            Secret = new string('a', 64),
            Issuer = "real-issuer",
            Audience = "audience",
            AccessTokenExpirationMinutes = 5,
            RefreshTokenExpirationDays = 14
        });
        var service = new TokenService(settings, NullLogger<TokenService>.Instance);
        var jwt = service.GenerateAccessToken(user, "User", null, null);

        var wrongSettings = Options.Create(new JwtSettings
        {
            Secret = new string('a', 64),
            Issuer = "other-issuer",
            Audience = "audience",
            AccessTokenExpirationMinutes = 5,
            RefreshTokenExpirationDays = 14
        });
        var serviceWrong = new TokenService(wrongSettings, NullLogger<TokenService>.Instance);

        var principal = serviceWrong.ValidateAccessToken(jwt);

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithWrongAudience_Fails()
    {
        var user = new MBCUser { Id = Guid.NewGuid(), Email = "aud@fail.com" };
        var settings = Options.Create(new JwtSettings
        {
            Secret = new string('a', 64),
            Issuer = "issuer",
            Audience = "expected",
            AccessTokenExpirationMinutes = 5,
            RefreshTokenExpirationDays = 14
        });
        var service = new TokenService(settings, NullLogger<TokenService>.Instance);
        var jwt = service.GenerateAccessToken(user, "User", null, null);

        var wrongSettings = Options.Create(new JwtSettings
        {
            Secret = new string('a', 64),
            Issuer = "issuer",
            Audience = "wrong",
            AccessTokenExpirationMinutes = 5,
            RefreshTokenExpirationDays = 14
        });
        var serviceWrong = new TokenService(wrongSettings, NullLogger<TokenService>.Instance);

        var principal = serviceWrong.ValidateAccessToken(jwt);

        Assert.Null(principal);
    }
}
