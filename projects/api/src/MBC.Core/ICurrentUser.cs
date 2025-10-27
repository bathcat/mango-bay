using System;
using System.Security.Claims;

namespace MBC.Core;

public interface ICurrentUser
{
    ClaimsPrincipal User { get; }
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid? CustomerId { get; }
    Guid? PilotId { get; }
    string Email { get; }
    string Role { get; }
}

