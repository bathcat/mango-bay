using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Core.Authorization;

public static class DeliveryProofOperations
{
    private const string Name = "DeliveryProof";

    public static readonly OperationAuthorizationRequirement Create = new() { Name = nameof(Create) };
    public static readonly OperationAuthorizationRequirement Read = AuthorizedFor.View.ToOperationAuthorizationRequirement(Name);
}

