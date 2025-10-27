using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Core.Services;

/// <summary>
/// Thin wrapper around IAuthorizationService.
/// Methods are synchronous (not async) to prevent accidentally forgetting to await,
/// which would cause authorization to be bypassed silently.
/// </summary>
public interface IMbcAuthorizationService
{
    void ThrowIfUnauthorized<TResource>(OperationAuthorizationRequirement operation, TResource resource);

    void ThrowIfUnauthorized(IEnumerable<string> authorizedRoles);
}
