using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Services.Authorization;

public class CreateDeliveryProofAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Delivery>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Delivery delivery)
    {
        if (requirement.Name != nameof(DeliveryProofOperations.Create))
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(UserRoles.Administrator))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var pilotIdClaim = context.User.FindFirst(MBCClaims.PilotId)?.Value;
        if (pilotIdClaim != null &&
            Guid.TryParse(pilotIdClaim, out var pilotId) &&
            delivery.PilotId == pilotId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

