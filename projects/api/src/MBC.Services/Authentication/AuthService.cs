using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBC.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<MBCUser> _userManager;
    private readonly Func<MBCUser, string, Task<SignInResult>> _checkPasswordSignInAsync;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ICustomerStore _customerStore;
    private readonly IPilotStore _pilotStore;
    private readonly IFingerprintService _fingerprintService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<MBCUser> userManager,
        SignInManager<MBCUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokenStore,
        ICustomerStore customerStore,
        IPilotStore pilotStore,
        IFingerprintService fingerprintService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
        : this(
            userManager,
            (user, password) => signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false),
            tokenService,
            refreshTokenStore,
            customerStore,
            pilotStore,
            fingerprintService,
            jwtSettings,
            logger)
    {
    }

    /// <summary>
    /// Test ctor. (Avoids having to mock SignInManager)
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="checkPasswordSignInAsync"></param>
    /// <param name="tokenService"></param>
    /// <param name="refreshTokenStore"></param>
    /// <param name="customerStore"></param>
    /// <param name="pilotStore"></param>
    /// <param name="fingerprintService"></param>
    /// <param name="jwtSettings"></param>
    /// <param name="logger"></param>
    public AuthService(
        UserManager<MBCUser> userManager,
        Func<MBCUser, string, Task<SignInResult>> checkPasswordSignInAsync,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokenStore,
        ICustomerStore customerStore,
        IPilotStore pilotStore,
        IFingerprintService fingerprintService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _checkPasswordSignInAsync = checkPasswordSignInAsync;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
        _customerStore = customerStore;
        _pilotStore = pilotStore;
        _fingerprintService = fingerprintService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResult> SignUp(string email, string password, string nickname)
    {
        _logger.LogInformation("Sign up attempt for {Email}", email);

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning("Sign up failed: Email {Email} already exists", email);
            return AuthResult.Failure("Email already exists");
        }

        var user = new MBCUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return AuthResult.Failure(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Customer role to user {UserId}, rolling back", user.Id);
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return AuthResult.Failure(errors);
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Nickname = nickname
        };

        await _customerStore.Create(customer);

        _logger.LogInformation("Successfully created customer {CustomerId} with user {UserId}", customer.Id, user.Id);

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> CreatePilot(string email, string password, string shortName, string fullName, string bio, string? avatarUrl)
    {
        _logger.LogInformation("Create pilot attempt for {Email}", email);

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning("Create pilot failed: Email {Email} already exists", email);
            return AuthResult.Failure("Email already exists");
        }

        var user = new MBCUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return AuthResult.Failure(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Pilot);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Pilot role to user {UserId}, rolling back", user.Id);
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return AuthResult.Failure(errors);
        }

        var pilot = new Pilot
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ShortName = shortName,
            FullName = fullName,
            Bio = bio,
            AvatarUrl = avatarUrl
        };

        await _pilotStore.Create(pilot);

        _logger.LogInformation("Successfully created pilot {PilotId} with user {UserId}", pilot.Id, user.Id);

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> CreateAdmin(string email, string password)
    {
        _logger.LogInformation("Create admin attempt for {Email}", email);

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning("Create admin failed: Email {Email} already exists", email);
            return AuthResult.Failure("Email already exists");
        }

        var user = new MBCUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return AuthResult.Failure(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Administrator);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Administrator role to user {UserId}, rolling back", user.Id);
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return AuthResult.Failure(errors);
        }

        _logger.LogInformation("Successfully created admin with user {UserId}", user.Id);

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> SignIn(string email, string password)
    {
        _logger.LogInformation("Sign in attempt for {Email}", email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Sign in failed: User {Email} not found", email);
            return AuthResult.Failure("Invalid email or password");
        }

        var result = await _checkPasswordSignInAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Sign in failed: Invalid password for {Email}", email);
            return AuthResult.Failure("Invalid email or password");
        }

        _logger.LogInformation("User {UserId} signed in successfully", user.Id);

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> RefreshToken(string refreshToken)
    {
        _logger.LogDebug("Refresh token attempt");

        var currentFingerprint = _fingerprintService.GenerateFingerprint();
        var tokenHash = _tokenService.HashToken(refreshToken);
        var result = await _refreshTokenStore.ConsumeToken(tokenHash);

        if (!result.Success)
        {
            if (result.FailureReason == TokenConsumeFailureReason.AlreadyConsumed && result.FamilyId.HasValue)
            {
                _logger.LogWarning("Token reuse detected! Revoking family {FamilyId}", result.FamilyId.Value);
                await _refreshTokenStore.RevokeFamily(result.FamilyId.Value);
            }

            if (result.FailureReason == TokenConsumeFailureReason.Expired)
            {
                _logger.LogWarning("Refresh token consumption failed: Token expired");
                return AuthResult.Failure("Refresh token expired");
            }

            _logger.LogWarning("Refresh token consumption failed: {Reason}", result.FailureReason);
            return AuthResult.Failure("Invalid refresh token");
        }

        var storedToken = result.Token!;

        if (currentFingerprint != storedToken.Fingerprint)
        {
            _logger.LogWarning("Fingerprint mismatch detected! Possible token theft. FamilyId: {FamilyId}, UserId: {UserId}, Expected: {Expected}, Actual: {Actual}",
                storedToken.FamilyId, storedToken.UserId, storedToken.Fingerprint.Value, currentFingerprint.Value);
            await _refreshTokenStore.RevokeFamily(storedToken.FamilyId);
            return AuthResult.Failure("Invalid refresh token");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for refresh token", storedToken.UserId);
            return AuthResult.Failure("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count != 1)
        {
            throw new InvalidOperationException($"Expected user to have exactly one role, but found {roles.Count}");
        }

        var role = roles[0];
        var customer = await _customerStore.GetByUserId(user.Id);
        var pilot = await _pilotStore.GetByUserId(user.Id);

        var (newRefreshToken, newTokenHash) = _tokenService.GenerateOpaqueRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = storedToken.FamilyId,
            TokenHash = newTokenHash,
            Fingerprint = currentFingerprint,
            ExpiresAt = storedToken.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenStore.Create(newRefreshTokenEntity);

        _logger.LogInformation("Refresh token rotated successfully. New TokenId: {TokenId}, UserId: {UserId}, FamilyId: {FamilyId}",
            newRefreshTokenEntity.Id, user.Id, storedToken.FamilyId);

        return AuthResult.SuccessResult(
            _tokenService.GenerateAccessToken(user, role, customer?.Id, pilot?.Id),
            newRefreshToken,
            user.Id,
            user.Email,
            customer?.Nickname,
            role,
            customer?.Id,
            pilot?.Id
        );
    }

    public async Task SignOut(string refreshToken)
    {
        _logger.LogInformation("Sign out requested");

        var currentFingerprint = _fingerprintService.GenerateFingerprint();
        var tokenHash = _tokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenStore.GetByTokenHash(tokenHash);

        if (storedToken == null)
        {
            _logger.LogDebug("Sign out: Token not found");
            return;
        }

        if (currentFingerprint != storedToken.Fingerprint)
        {
            _logger.LogWarning("Sign out fingerprint mismatch! Possible token compromise. FamilyId: {FamilyId}, UserId: {UserId}, Expected: {Expected}, Actual: {Actual}",
                storedToken.FamilyId, storedToken.UserId, storedToken.Fingerprint.Value, currentFingerprint.Value);
            await _refreshTokenStore.RevokeFamily(storedToken.FamilyId);
            _logger.LogWarning("Revoked entire family {FamilyId} due to fingerprint mismatch", storedToken.FamilyId);
            return;
        }

        await _refreshTokenStore.TurnIn(tokenHash);
        _logger.LogInformation("Successfully signed out. FamilyId: {FamilyId}, UserId: {UserId}", storedToken.FamilyId, storedToken.UserId);
    }


    private async Task<AuthResult> GenerateAuthResult(MBCUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count != 1)
        {
            throw new InvalidOperationException($"Expected user to have exactly one role, but found {roles.Count}");
        }

        var role = roles[0];
        var customer = await _customerStore.GetByUserId(user.Id);
        var pilot = await _pilotStore.GetByUserId(user.Id);

        var fingerprint = _fingerprintService.GenerateFingerprint();
        var tokenId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        var (refreshToken, tokenHash) = _tokenService.GenerateOpaqueRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = tokenId,
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = tokenHash,
            Fingerprint = fingerprint,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenStore.Create(refreshTokenEntity);

        _logger.LogInformation("Created refresh token {TokenId} for user {UserId}, FamilyId: {FamilyId}, expires at {ExpiresAt}",
            tokenId, user.Id, familyId, expiresAt);

        return AuthResult.SuccessResult(
            _tokenService.GenerateAccessToken(user, role, customer?.Id, pilot?.Id),
            refreshToken,
            user.Id,
            user.Email,
            customer?.Nickname,
            role,
            customer?.Id,
            pilot?.Id
        );
    }
}

