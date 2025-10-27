using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MBC.Services.Authorization;

public class StakeholderAuthorizationHandler<TResource>
    : AuthorizationHandler<OperationAuthorizationRequirement, TResource>
    where TResource : class
{
    private readonly StakeholderAuthorizationStrategy _strategy;

    public StakeholderAuthorizationHandler()
    {
        _strategy = Strategies.All.FirstOrDefault(s => s.ResourceType == typeof(TResource))
            ?? throw new InvalidOperationException(
                $"No StakeholderAuthorizationStrategy found for resource type {typeof(TResource).Name}. " +
                $"Add a strategy to Strategies.All.");
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        TResource resource)
    {
        var authorizedFor = AuthorizedForExtensions.ParseFromOperationName(requirement.Name);
        if (authorizedFor == null)
        {
            return Task.CompletedTask;
        }

        var allowedOperations = DetermineStakeholderRole(context, resource);

        if (allowedOperations.HasFlag(authorizedFor.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private AuthorizedFor DetermineStakeholderRole(
        AuthorizationHandlerContext context,
        TResource resource)
    {
        if (context.User.IsInRole(UserRoles.Administrator))
        {
            return _strategy.Admin;
        }

        if (resource is ICustomerOwned customerOwned &&
            IsCurrentCustomer(context, customerOwned.CustomerId))
        {
            return _strategy.Customer;
        }

        if (resource is IPilotAssigned pilotAssigned &&
            IsCurrentPilot(context, pilotAssigned.PilotId))
        {
            return _strategy.Pilot;
        }

        return _strategy.Anonymous;
    }

    private static bool IsCurrentCustomer(AuthorizationHandlerContext context, Guid customerId)
    {
        var customerIdClaim = context.User.FindFirst(MBCClaims.CustomerId)?.Value;
        return customerIdClaim != null &&
               Guid.TryParse(customerIdClaim, out var currentCustomerId) &&
               currentCustomerId == customerId;
    }

    private static bool IsCurrentPilot(AuthorizationHandlerContext context, Guid pilotId)
    {
        var pilotIdClaim = context.User.FindFirst(MBCClaims.PilotId)?.Value;
        return pilotIdClaim != null &&
               Guid.TryParse(pilotIdClaim, out var currentPilotId) &&
               currentPilotId == pilotId;
    }
}

