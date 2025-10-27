using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MBC.Endpoints.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var customersGroup = app.MapGroup(ApiRoutes.Customers)
            .WithTags("Customers")
            .RequireAuthorization();

        customersGroup.MapGet("/{id}", GetCustomer)
            .WithName("GetCustomer")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific customer by their ID.");

        customersGroup.MapPut("/{id}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates a customer's information.");
    }

    public static async Task<Results<Ok<CustomerDto>, NotFound>> GetCustomer(
        ICustomerService customerService,
        ICurrentUser currentUser,
        IMapper<Customer, CustomerDto> customerMapper,
        Guid id)
    {
        var canViewAnyCustomer = currentUser.User.IsInRole(UserRoles.Administrator) ||
                                 currentUser.User.IsInRole(UserRoles.Pilot);

        var canViewSelf = currentUser.User.IsInRole(UserRoles.Customer) &&
                          currentUser.CustomerId == id;

        if (!canViewAnyCustomer && !canViewSelf)
        {
            return TypedResults.NotFound();
        }

        var customer = await customerService.GetCustomerById(id);
        if (customer == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(customerMapper.Map(customer));
    }

    public static async Task<Results<Ok<CustomerDto>, NotFound>> UpdateCustomer(
        ICustomerService customerService,
        ICurrentUser currentUser,
        IMapper<Customer, CustomerDto> customerMapper,
        Guid id,
        UpdateCustomerRequest request)
    {
        var canUpdateAnyCustomer = currentUser.User.IsInRole(UserRoles.Administrator);

        var canUpdateSelf = currentUser.User.IsInRole(UserRoles.Customer) &&
                            currentUser.CustomerId == id;

        if (!canUpdateAnyCustomer && !canUpdateSelf)
        {
            return TypedResults.NotFound();
        }

        var customer = await customerService.UpdateCustomer(id, request.Nickname);
        var customerDto = customerMapper.Map(customer);
        return TypedResults.Ok(customerDto);
    }
}

