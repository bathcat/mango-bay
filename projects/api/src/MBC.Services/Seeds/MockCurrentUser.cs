using System;
using System.Collections.Generic;
using System.Security.Claims;
using MBC.Core;

namespace MBC.Services.Seeds;

public class MockCurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public MockCurrentUser(Guid userId, string role, Guid? customerId = null, Guid? pilotId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        if (customerId.HasValue)
        {
            claims.Add(new(MBCClaims.CustomerId, customerId.Value.ToString()));
        }

        if (pilotId.HasValue)
        {
            claims.Add(new(MBCClaims.PilotId, pilotId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Mock");
        _principal = new ClaimsPrincipal(identity);
    }

    public ClaimsPrincipal User => _principal;
    public bool IsAuthenticated => true;
    public Guid UserId => Guid.Parse(_principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("No user ID claim"));
    public Guid? CustomerId => _principal.FindFirst(MBCClaims.CustomerId)?.Value is { } value ? Guid.Parse(value) : null;
    public Guid? PilotId => _principal.FindFirst(MBCClaims.PilotId)?.Value is { } value ? Guid.Parse(value) : null;
    public string Email => _principal.FindFirst(ClaimTypes.Email)?.Value ?? "mock@example.com";
    public string Role => _principal.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
}
