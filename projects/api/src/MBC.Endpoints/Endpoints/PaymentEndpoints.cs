using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

/// <summary>
/// Payment endpoints with no service layer abstraction.
/// </summary>
/// <remarks>
/// Authorization strategy: imperative checks directly in the minimal API endpoints.
/// * Pros: Authorization visible right where HTTP requests are handled.
/// * Cons: Duplicative, harder to test in isolation, mixes HTTP and authorization concerns.
///
/// Compare with CustomerService (imperative in service layer) and DeliveryService
/// (delegated to IMbcAuthorizationService).
/// </remarks>
public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var paymentsGroup = app.MapGroup(ApiRoutes.Payments)
            .RequireAuthorization()
            .WithTags("Payments");

        paymentsGroup.MapGet("/{id}", GetPayment)
            .WithName("GetPayment")
            .Produces<PaymentDto?>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a specific payment by its ID. Returns null if not found or not authorized.");

        paymentsGroup.MapGet("/by-delivery/{deliveryId}", GetPaymentByDeliveryId)
            .WithName("GetPaymentByDeliveryId")
            .Produces<PaymentDto?>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a payment by its associated delivery ID. Returns null if not found or not authorized.");

        paymentsGroup.MapGet("/search-by-cardholders", SearchByCardholderNames)
            .WithName("SearchPaymentsByCardholderNames")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK)
            .WithDescription("Searches payments by cardholder names for the current customer.");
    }

    public static async Task<Ok<PaymentDto?>> GetPayment(
        IPaymentStore paymentStore,
        IDeliveryStore deliveryStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        ICurrentUser currentUser,
        Guid id)
    {
        var payment = await paymentStore.GetById(id);
        if (payment == null)
        {
            return TypedResults.Ok<PaymentDto?>(null);
        }

        var isAdmin = currentUser.User.IsInRole(UserRoles.Administrator);
        var isCustomer = currentUser.User.IsInRole(UserRoles.Customer);

        if (!isAdmin && !isCustomer)
        {
            return TypedResults.Ok<PaymentDto?>(null);
        }

        if (!isAdmin)
        {
            var delivery = await deliveryStore.GetById(payment.DeliveryId);
            if (delivery?.CustomerId != currentUser.CustomerId)
            {
                return TypedResults.Ok<PaymentDto?>(null);
            }
        }

        return TypedResults.Ok<PaymentDto?>(paymentMapper.Map(payment));
    }

    public static async Task<Ok<PaymentDto?>> GetPaymentByDeliveryId(
        IPaymentStore paymentStore,
        IDeliveryStore deliveryStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        ICurrentUser currentUser,
        Guid deliveryId)
    {
        var payment = await paymentStore.GetByDeliveryId(deliveryId);
        if (payment == null)
        {
            return TypedResults.Ok<PaymentDto?>(null);
        }

        var isAdmin = currentUser.User.IsInRole(UserRoles.Administrator);
        var isCustomer = currentUser.User.IsInRole(UserRoles.Customer);

        if (!isAdmin && !isCustomer)
        {
            return TypedResults.Ok<PaymentDto?>(null);
        }

        if (!isAdmin)
        {
            var delivery = await deliveryStore.GetById(deliveryId);
            if (delivery?.CustomerId != currentUser.CustomerId)
            {
                return TypedResults.Ok<PaymentDto?>(null);
            }
        }

        return TypedResults.Ok<PaymentDto?>(paymentMapper.Map(payment));
    }

    public static async Task<Ok<IEnumerable<PaymentDto>>> SearchByCardholderNames(
        IPaymentStore paymentStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        ICurrentUser currentUser,
        [FromQuery(Name = "names")] string? namesParam)
    {
        var isCustomer = currentUser.User.IsInRole(UserRoles.Customer);

        if (!isCustomer || currentUser.CustomerId == null)
        {
            throw new UnauthorizedAccessException("Only customers can search their payments.");
        }

        var names = string.IsNullOrWhiteSpace(namesParam)
            ? []
            : namesParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var payments = await paymentStore.SearchByCardholderNames(currentUser.CustomerId.Value, names);
        var paymentDtos = payments.Select(paymentMapper.Map);
        return TypedResults.Ok(paymentDtos);
    }
}
