using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Core.ValueObjects;
using MBC.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MBC.Services.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task SignUp_WithValidData_CreatesCustomerAndReturnsSuccess()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fingerprint = Fingerprint.From("test-fingerprint");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var tokenHash = "token-hash";

        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((MBCUser?)null);

        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<MBCUser>(), "password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<MBCUser, string>((user, _) => user.Id = userId);

        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<MBCUser>(), "Customer"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<MBCUser>()))
            .ReturnsAsync(new List<string> { "Customer" });

        customerStoreMock.Setup(x => x.Create(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => { c.Id = customerId; return c; });

        customerStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId, Nickname = "TestUser" });

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        tokenServiceMock.Setup(x => x.GenerateOpaqueRefreshToken())
            .Returns((refreshToken, tokenHash));

        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<MBCUser>(), "Customer", customerId, null))
            .Returns(accessToken);

        refreshTokenStoreMock.Setup(x => x.Create(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignUp("test@example.com", "password123", "TestUser");

        Assert.True(result.Success);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("TestUser", result.Nickname);
        Assert.Equal("Customer", result.Role);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Null(result.PilotId);

        customerStoreMock.Verify(x => x.Create(It.Is<Customer>(c =>
            c.UserId == userId && c.Nickname == "TestUser")), Times.Once);
    }

    [Fact]
    public async Task SignUp_WithExistingEmail_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var existingUser = new MBCUser { Id = Guid.NewGuid(), Email = "test@example.com" };
        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignUp("test@example.com", "password123", "TestUser");

        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.ErrorMessage);
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<MBCUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignUp_WhenUserCreationFails_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((MBCUser?)null);

        var error = new IdentityError { Description = "Password requires digit" };
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<MBCUser>(), "weak"))
            .ReturnsAsync(IdentityResult.Failed(error));

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignUp("test@example.com", "weak", "TestUser");

        Assert.False(result.Success);
        Assert.Contains("Password requires digit", result.ErrorMessage);
        customerStoreMock.Verify(x => x.Create(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task CreatePilot_WithValidData_CreatesPilotAndReturnsSuccess()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var fingerprint = Fingerprint.From("test-fingerprint");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var tokenHash = "token-hash";

        userManagerMock.Setup(x => x.FindByEmailAsync("pilot@example.com"))
            .ReturnsAsync((MBCUser?)null);

        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<MBCUser>(), "password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<MBCUser, string>((user, _) => user.Id = userId);

        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<MBCUser>(), "Pilot"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<MBCUser>()))
            .ReturnsAsync(new List<string> { "Pilot" });

        pilotStoreMock.Setup(x => x.Create(It.IsAny<Pilot>()))
            .ReturnsAsync((Pilot p) => { p.Id = pilotId; return p; });

        pilotStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new Pilot { Id = pilotId, UserId = userId, ShortName = "Sky", FullName = "Sky Captain" });

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        tokenServiceMock.Setup(x => x.GenerateOpaqueRefreshToken())
            .Returns((refreshToken, tokenHash));

        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<MBCUser>(), "Pilot", null, pilotId))
            .Returns(accessToken);

        refreshTokenStoreMock.Setup(x => x.Create(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.CreatePilot("pilot@example.com", "password123", "Sky", "Sky Captain", "Experienced pilot", "avatar.jpg");

        Assert.True(result.Success);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Pilot", result.Role);
        Assert.Equal(pilotId, result.PilotId);
        Assert.Null(result.CustomerId);

        pilotStoreMock.Verify(x => x.Create(It.Is<Pilot>(p =>
            p.UserId == userId &&
            p.ShortName == "Sky" &&
            p.FullName == "Sky Captain" &&
            p.Bio == "Experienced pilot" &&
            p.AvatarUrl == "avatar.jpg")), Times.Once);
    }

    [Fact]
    public async Task CreatePilot_WithExistingEmail_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var existingUser = new MBCUser { Id = Guid.NewGuid(), Email = "pilot@example.com" };
        userManagerMock.Setup(x => x.FindByEmailAsync("pilot@example.com"))
            .ReturnsAsync(existingUser);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.CreatePilot("pilot@example.com", "password123", "Sky", "Sky Captain", "Bio", null);

        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.ErrorMessage);
        pilotStoreMock.Verify(x => x.Create(It.IsAny<Pilot>()), Times.Never);
    }

    [Fact]
    public async Task CreateAdmin_WithValidData_CreatesAdminAndReturnsSuccess()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var fingerprint = Fingerprint.From("test-fingerprint");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var tokenHash = "token-hash";

        userManagerMock.Setup(x => x.FindByEmailAsync("admin@example.com"))
            .ReturnsAsync((MBCUser?)null);

        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<MBCUser>(), "password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<MBCUser, string>((user, _) => user.Id = userId);

        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<MBCUser>(), "Administrator"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<MBCUser>()))
            .ReturnsAsync(new List<string> { "Administrator" });

        customerStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync((Customer?)null);

        pilotStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync((Pilot?)null);

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        tokenServiceMock.Setup(x => x.GenerateOpaqueRefreshToken())
            .Returns((refreshToken, tokenHash));

        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<MBCUser>(), "Administrator", null, null))
            .Returns(accessToken);

        refreshTokenStoreMock.Setup(x => x.Create(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.CreateAdmin("admin@example.com", "password123");

        Assert.True(result.Success);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Administrator", result.Role);
        Assert.Null(result.CustomerId);
        Assert.Null(result.PilotId);
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsSuccess()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var user = new MBCUser { Id = userId, Email = "test@example.com", UserName = "test@example.com" };
        var fingerprint = Fingerprint.From("test-fingerprint");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var tokenHash = "token-hash";

        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Customer" });

        customerStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId, Nickname = "TestUser" });

        pilotStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync((Pilot?)null);

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        tokenServiceMock.Setup(x => x.GenerateOpaqueRefreshToken())
            .Returns((refreshToken, tokenHash));

        tokenServiceMock.Setup(x => x.GenerateAccessToken(user, "Customer", customerId, null))
            .Returns(accessToken);

        refreshTokenStoreMock.Setup(x => x.Create(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var sut = new AuthService(
            userManagerMock.Object,
            (u, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignIn("test@example.com", "correctPassword");

        Assert.True(result.Success);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task SignIn_WithNonExistentUser_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        userManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((MBCUser?)null);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignIn("nonexistent@example.com", "password");

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.ErrorMessage);
    }

    [Fact]
    public async Task SignIn_WithInvalidPassword_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var user = new MBCUser { Id = Guid.NewGuid(), Email = "test@example.com" };
        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        var sut = new AuthService(
            userManagerMock.Object,
            (u, password) => Task.FromResult(SignInResult.Failed),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.SignIn("test@example.com", "wrongPassword");

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_RotatesTokenAndReturnsNewTokens()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var oldTokenHash = "old-token-hash";
        var newRefreshToken = "new-refresh-token";
        var newTokenHash = "new-token-hash";
        var fingerprint = Fingerprint.From("test-fingerprint");
        var accessToken = "new-access-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = oldTokenHash,
            Fingerprint = fingerprint,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var user = new MBCUser { Id = userId, Email = "test@example.com" };

        tokenServiceMock.Setup(x => x.HashToken("old-refresh-token"))
            .Returns(oldTokenHash);

        refreshTokenStoreMock.Setup(x => x.ConsumeToken(oldTokenHash))
            .ReturnsAsync(ConsumeTokenResult.Succeeded(storedToken));

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Customer" });

        customerStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId, Nickname = "TestUser" });

        pilotStoreMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync((Pilot?)null);

        tokenServiceMock.Setup(x => x.GenerateOpaqueRefreshToken())
            .Returns((newRefreshToken, newTokenHash));

        tokenServiceMock.Setup(x => x.GenerateAccessToken(user, "Customer", customerId, null))
            .Returns(accessToken);

        refreshTokenStoreMock.Setup(x => x.Create(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var sut = new AuthService(
            userManagerMock.Object,
            (u, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.RefreshToken("old-refresh-token");

        Assert.True(result.Success);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        Assert.Equal(userId, result.UserId);

        refreshTokenStoreMock.Verify(x => x.Create(It.Is<RefreshToken>(t =>
            t.UserId == userId &&
            t.FamilyId == familyId &&
            t.TokenHash == newTokenHash &&
            t.Fingerprint == fingerprint &&
            t.ExpiresAt == storedToken.ExpiresAt)), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithReusedToken_RevokesEntireFamily()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var familyId = Guid.NewGuid();
        var tokenHash = "reused-token-hash";

        tokenServiceMock.Setup(x => x.HashToken("reused-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.ConsumeToken(tokenHash))
            .ReturnsAsync(ConsumeTokenResult.Failed(TokenConsumeFailureReason.AlreadyConsumed, familyId));

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.RefreshToken("reused-token");

        Assert.False(result.Success);
        Assert.Equal("Invalid refresh token", result.ErrorMessage);
        refreshTokenStoreMock.Verify(x => x.RevokeFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var tokenHash = "expired-token-hash";

        tokenServiceMock.Setup(x => x.HashToken("expired-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.ConsumeToken(tokenHash))
            .ReturnsAsync(ConsumeTokenResult.Failed(TokenConsumeFailureReason.Expired));

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.RefreshToken("expired-token");

        Assert.False(result.Success);
        Assert.Equal("Refresh token expired", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshToken_WithFingerprintMismatch_RevokesFamily()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenHash = "token-hash";
        var storedFingerprint = Fingerprint.From("stored-fingerprint");
        var currentFingerprint = Fingerprint.From("different-fingerprint");

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            Fingerprint = storedFingerprint,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        tokenServiceMock.Setup(x => x.HashToken("stolen-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.ConsumeToken(tokenHash))
            .ReturnsAsync(ConsumeTokenResult.Succeeded(storedToken));

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(currentFingerprint);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.RefreshToken("stolen-token");

        Assert.False(result.Success);
        Assert.Equal("Invalid refresh token", result.ErrorMessage);
        refreshTokenStoreMock.Verify(x => x.RevokeFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithNonExistentUser_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenHash = "token-hash";
        var fingerprint = Fingerprint.From("test-fingerprint");

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            Fingerprint = fingerprint,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        tokenServiceMock.Setup(x => x.HashToken("orphaned-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.ConsumeToken(tokenHash))
            .ReturnsAsync(ConsumeTokenResult.Succeeded(storedToken));

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((MBCUser?)null);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        var result = await sut.RefreshToken("orphaned-token");

        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task SignOut_WithValidToken_RevokesToken()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenHash = "token-hash";
        var fingerprint = Fingerprint.From("test-fingerprint");

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            Fingerprint = fingerprint,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        tokenServiceMock.Setup(x => x.HashToken("valid-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.GetByTokenHash(tokenHash))
            .ReturnsAsync(storedToken);

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(fingerprint);

        refreshTokenStoreMock.Setup(x => x.TurnIn(tokenHash))
            .Returns(Task.CompletedTask);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        await sut.SignOut("valid-token");

        refreshTokenStoreMock.Verify(x => x.TurnIn(tokenHash), Times.Once);
        refreshTokenStoreMock.Verify(x => x.RevokeFamily(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SignOut_WithFingerprintMismatch_RevokesEntireFamily()
    {
        var userManagerMock = CreateUserManagerMock();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
        var customerStoreMock = new Mock<ICustomerStore>();
        var pilotStoreMock = new Mock<IPilotStore>();
        var fingerprintServiceMock = new Mock<IFingerprintService>();
        var jwtSettings = CreateJwtSettings();

        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenHash = "token-hash";
        var storedFingerprint = Fingerprint.From("stored-fingerprint");
        var currentFingerprint = Fingerprint.From("different-fingerprint");

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            Fingerprint = storedFingerprint,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        tokenServiceMock.Setup(x => x.HashToken("compromised-token"))
            .Returns(tokenHash);

        refreshTokenStoreMock.Setup(x => x.GetByTokenHash(tokenHash))
            .ReturnsAsync(storedToken);

        fingerprintServiceMock.Setup(x => x.GenerateFingerprint())
            .Returns(currentFingerprint);

        var sut = new AuthService(
            userManagerMock.Object,
            (user, password) => Task.FromResult(SignInResult.Success),
            tokenServiceMock.Object,
            refreshTokenStoreMock.Object,
            customerStoreMock.Object,
            pilotStoreMock.Object,
            fingerprintServiceMock.Object,
            jwtSettings,
            NullLogger<AuthService>.Instance);

        await sut.SignOut("compromised-token");

        refreshTokenStoreMock.Verify(x => x.RevokeFamily(familyId), Times.Once);
        refreshTokenStoreMock.Verify(x => x.TurnIn(It.IsAny<string>()), Times.Never);
    }

    private static Mock<UserManager<MBCUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<MBCUser>>();
        var mock = new Mock<UserManager<MBCUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
        return mock;
    }

    private static IOptions<JwtSettings> CreateJwtSettings()
    {
        return Options.Create(new JwtSettings
        {
            Secret = "test-secret-key-that-is-long-enough-for-hmac-sha256",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 30,
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
    }
}

