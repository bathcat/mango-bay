using System;
using System.Collections.Generic;
using System.Linq;
using MBC.Core;
using MBC.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Authorization;

/// <summary>
/// Thin wrapper around IAuthorizationService that pulls out the current user.
/// </summary>
/// <remarks>
/// Uses synchronous methods (with .GetAwaiter().GetResult()) to prevent forgetting to await.
/// While blocking on async is normally an anti-pattern, the safety benefit outweighs the concern
/// since authorization checks are fast in-memory operations.
/// </remarks>
public class MbcAuthorizationService : IMbcAuthorizationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<MbcAuthorizationService> _logger;

    public MbcAuthorizationService(
        IAuthorizationService authorizationService,
        ICurrentUser currentUser,
        ILogger<MbcAuthorizationService> logger)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ThrowIfUnauthorized<TResource>(
        OperationAuthorizationRequirement operation,
        TResource resource)
    {
        _logger.LogDebug("Checking authorization for user {UserId} with roles {Roles} on resource {ResourceType} for operation {Operation}",
            _currentUser.UserId,
            string.Join(",", _currentUser.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)),
            typeof(TResource).Name,
            operation.Name);

        var result = _authorizationService.AuthorizeAsync(
            _currentUser.User,
            resource,
            operation).GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            _logger.LogWarning("Authorization failed for user {UserId} on {ResourceType} for operation {Operation}. Reasons: {Reasons}",
                _currentUser.UserId,
                typeof(TResource).Name,
                operation.Name,
                string.Join(", ", result.Failure?.FailedRequirements?.Select(f => f.GetType().Name) ?? new[] { "Unknown" }));
            throw new UnauthorizedAccessException("Not authorized to perform this operation.");
        }

        _logger.LogDebug("Authorization succeeded for user {UserId} on {ResourceType} for operation {Operation}",
            _currentUser.UserId,
            typeof(TResource).Name,
            operation.Name);
    }

    public void ThrowIfUnauthorized(IEnumerable<string> authorizedRoles)
    {
        var userRoles = _currentUser.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var authorizedRolesList = authorizedRoles.ToList();

        _logger.LogDebug("Checking role authorization for user {UserId} with roles {UserRoles} against required roles {RequiredRoles}",
            _currentUser.UserId,
            string.Join(",", userRoles),
            string.Join(",", authorizedRolesList));

        if (!authorizedRolesList.Any(role => _currentUser.User.IsInRole(role)))
        {
            _logger.LogWarning("Role authorization failed for user {UserId} with roles {UserRoles}. Required one of: {RequiredRoles}",
                _currentUser.UserId,
                string.Join(",", userRoles),
                string.Join(",", authorizedRolesList));
            throw new UnauthorizedAccessException("Not authorized to perform this operation.");
        }

        _logger.LogDebug("Role authorization succeeded for user {UserId} with roles {UserRoles}",
            _currentUser.UserId,
            string.Join(",", userRoles));
    }
}
