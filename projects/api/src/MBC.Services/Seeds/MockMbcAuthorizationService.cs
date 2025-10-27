using System.Collections.Generic;
using MBC.Core.Services;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Services.Seeds;

public class MockMbcAuthorizationService : IMbcAuthorizationService
{
    public void ThrowIfUnauthorized<TResource>(OperationAuthorizationRequirement operation, TResource resource)
    {
        // Always succeed for seeding
    }

    public void ThrowIfUnauthorized(IEnumerable<string> authorizedRoles)
    {
        // Always succeed for seeding
    }
}
