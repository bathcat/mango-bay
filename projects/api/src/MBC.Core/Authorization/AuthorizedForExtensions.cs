using System;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Core.Authorization;

public static class AuthorizedForExtensions
{
    public static OperationAuthorizationRequirement ToOperationAuthorizationRequirement(
        this AuthorizedFor authorizedFor,
        string resourceName)
    {
        return new OperationAuthorizationRequirement { Name = $"{resourceName}.{authorizedFor}" };
    }

    public static AuthorizedFor? ParseFromOperationName(string operationName)
    {
        var parts = operationName.Split('.');
        if (parts.Length == 2 && Enum.TryParse<AuthorizedFor>(parts[1], out var authorizedFor))
        {
            return authorizedFor;
        }

        return null;
    }
}

