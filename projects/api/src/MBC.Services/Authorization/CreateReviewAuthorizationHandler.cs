using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Services.Authorization;

public class CreateReviewAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Delivery>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Delivery delivery)
    {
        if (requirement.Name != nameof(ReviewOperations.Create))
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(UserRoles.Administrator))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var customerIdClaim = context.User.FindFirst(MBCClaims.CustomerId)?.Value;
        if (customerIdClaim != null &&
            Guid.TryParse(customerIdClaim, out var customerId) &&
            delivery.CustomerId == customerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

