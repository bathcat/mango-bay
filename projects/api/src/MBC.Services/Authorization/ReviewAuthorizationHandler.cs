using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Services.Authorization;

/// <summary>
/// Review-specific authorization handler. Superceded by StakeholderAuthorizationHandler
/// Kept around for demo purposes.
/// </summary>
public class ReviewAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, DeliveryReview>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        DeliveryReview review)
    {
        if (requirement.Name != nameof(ReviewOperations.Update) &&
            requirement.Name != nameof(ReviewOperations.Delete))
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(UserRoles.Administrator))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var allowed = requirement.Name switch
        {
            nameof(ReviewOperations.Update) => CanUpdate(context, review),
            nameof(ReviewOperations.Delete) => CanUpdate(context, review),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool CanUpdate(AuthorizationHandlerContext context, DeliveryReview review)
    {
        var customerIdClaim = context.User.FindFirst(MBCClaims.CustomerId)?.Value;
        if (customerIdClaim != null &&
            Guid.TryParse(customerIdClaim, out var customerId) &&
            review.CustomerId == customerId)
        {
            return true;
        }

        return false;
    }
}

