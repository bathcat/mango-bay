using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Core.Authorization;

public static class ReviewOperations
{
    private const string Name = "Review";

    public static readonly OperationAuthorizationRequirement Create = new() { Name = nameof(Create) };
    public static readonly OperationAuthorizationRequirement Read = AuthorizedFor.View.ToOperationAuthorizationRequirement(Name);
    public static readonly OperationAuthorizationRequirement Update = AuthorizedFor.Mutate.ToOperationAuthorizationRequirement(Name);
    public static readonly OperationAuthorizationRequirement Delete = AuthorizedFor.Mutate.ToOperationAuthorizationRequirement(Name);
}

