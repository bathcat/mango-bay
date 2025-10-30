using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MBC.Core;
using Microsoft.AspNetCore.Http;

namespace MBC.Endpoints.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal User
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new InvalidOperationException("No user context available.");
            }
            return user;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated ?? false;
        }
    }

    public Guid UserId
    {
        get
        {
            EnsureAuthenticated();
            var userIdClaim = GetClaim(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("UserId claim (NameIdentifier) not found in token. This indicates a token generation defect.");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException($"UserId claim (NameIdentifier) value '{userIdClaim}' is not a valid GUID. This indicates a token generation defect.");
            }

            return userId;
        }
    }

    public Guid? CustomerId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var customerIdClaim = GetClaim(MBCClaims.CustomerId);
            if (string.IsNullOrEmpty(customerIdClaim))
            {
                return null;
            }

            if (!Guid.TryParse(customerIdClaim, out var customerId))
            {
                throw new InvalidOperationException($"CustomerId claim '{MBCClaims.CustomerId}' value '{customerIdClaim}' is not a valid GUID. This indicates a token generation defect.");
            }

            return customerId;
        }
    }

    public Guid? PilotId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var pilotIdClaim = GetClaim(MBCClaims.PilotId);
            if (string.IsNullOrEmpty(pilotIdClaim))
            {
                return null;
            }

            if (!Guid.TryParse(pilotIdClaim, out var pilotId))
            {
                throw new InvalidOperationException($"PilotId claim '{MBCClaims.PilotId}' value '{pilotIdClaim}' is not a valid GUID. This indicates a token generation defect.");
            }

            return pilotId;
        }
    }

    public string Email
    {
        get
        {
            EnsureAuthenticated();
            var email = GetClaim(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("Email claim not found in token. This indicates a token generation defect.");
            }

            return email;
        }
    }

    public string Role
    {
        get
        {
            EnsureAuthenticated();
            var user = _httpContextAccessor.HttpContext?.User;
            var roles = user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            if (roles.Count == 0)
            {
                throw new InvalidOperationException("No role found in token. This indicates a token generation defect.");
            }

            if (roles.Count > 1)
            {
                throw new InvalidOperationException($"Expected exactly one role, but found {roles.Count}. This indicates a token generation defect.");
            }

            return roles[0];
        }
    }

    private void EnsureAuthenticated()
    {
        if (!IsAuthenticated)
        {
            throw new InvalidOperationException("Cannot access user claims when not authenticated.");
        }
    }

    private string? GetClaim(string claimType)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(claimType)?.Value;
    }
}

