using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MBC.Endpoints.Endpoints;

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
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific payment by its ID.");

        paymentsGroup.MapGet("/by-delivery/{deliveryId}", GetPaymentByDeliveryId)
            .WithName("GetPaymentByDeliveryId")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a payment by its associated delivery ID.");
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
}
