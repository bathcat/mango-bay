using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Core.Authorization;

public static class DeliveryOperations
{
    private const string Name = "Delivery";

    public static readonly OperationAuthorizationRequirement Read = AuthorizedFor.View.ToOperationAuthorizationRequirement(Name);
    public static readonly OperationAuthorizationRequirement Update = AuthorizedFor.Mutate.ToOperationAuthorizationRequirement(Name);
}

