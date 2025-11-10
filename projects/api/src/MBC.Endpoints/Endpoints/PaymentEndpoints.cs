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

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var paymentsGroup = app.MapGroup(ApiRoutes.Payments)
            .RequireAuthorization()
            .WithTags("Payments");

        //TODO: Authorize these.
        paymentsGroup.MapGet("/{id}", GetPayment)
            .WithName("GetPayment")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific payment by its ID.");

        paymentsGroup.MapGet("/by-delivery/{deliveryId}", GetPaymentByDeliveryId)
            .WithName("GetPaymentByDeliveryId")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a payment by its associated delivery ID.");

        paymentsGroup.MapGet("/search-by-cardholders", SearchByCardholderNames)
            .WithName("SearchPaymentsByCardholderNames")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK)
            .WithDescription("Searches payments by cardholder names for the current customer.");
    }

    public static async Task<Results<Ok<PaymentDto>, NotFound>> GetPayment(
        IPaymentStore paymentStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        Guid id)
    {
        var payment = await paymentStore.GetById(id);
        if (payment == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(paymentMapper.Map(payment));
    }

    public static async Task<Results<Ok<PaymentDto>, NotFound>> GetPaymentByDeliveryId(
        IPaymentStore paymentStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        Guid deliveryId)
    {
        var payment = await paymentStore.GetByDeliveryId(deliveryId);
        if (payment == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(paymentMapper.Map(payment));
    }

    public static async Task<Ok<IEnumerable<PaymentDto>>> SearchByCardholderNames(
        IPaymentStore paymentStore,
        IMapper<Payment, PaymentDto> paymentMapper,
        ICurrentUser currentUser,
        [FromQuery(Name = "names")] string? namesParam)
    {
        if (currentUser.CustomerId == null)
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
