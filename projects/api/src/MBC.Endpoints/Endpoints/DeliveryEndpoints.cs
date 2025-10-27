using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

public static class DeliveryEndpoints
{
    public static void MapDeliveryEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var deliveriesGroup = app.MapGroup(ApiRoutes.Deliveries)
            .RequireAuthorization()
            .WithTags("Deliveries");

        deliveriesGroup.MapPost("/", BookDelivery)
            .WithName("BookDelivery")
            .Produces<DeliveryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Books a new cargo delivery.");

        deliveriesGroup.MapGet("/{id}", GetDelivery)
            .WithName("GetDelivery")
            .Produces<DeliveryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific delivery by its ID.");

        deliveriesGroup.MapGet("/my-deliveries", GetDeliveriesForCurrentCustomer)
            .WithName("GetDeliveriesForCurrentCustomer")
            .Produces<Page<DeliveryDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves all deliveries for the currently authenticated customer.");

        deliveriesGroup.MapGet("/my-assignments", GetDeliveriesForCurrentPilot)
            .WithName("GetDeliveriesForCurrentPilot")
            .Produces<Page<DeliveryDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves all deliveries for the currently authenticated pilot.");

        deliveriesGroup.MapGet("/pilot/{pilotId}", GetDeliveriesByPilot)
            .WithName("GetDeliveriesByPilot")
            .Produces<Page<DeliveryDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves all deliveries assigned to a specific pilot.");

        deliveriesGroup.MapDelete("/{id}", CancelDelivery)
            .WithName("CancelDelivery")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Cancels a delivery and issues a refund if applicable.");

        deliveriesGroup.MapPut("/{id}/status", UpdateDeliveryStatus)
            .WithName("UpdateDeliveryStatus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates the status of a delivery.");

        deliveriesGroup.MapPost("/calculate-cost", CalculateCost)
            .WithName("CalculateCost")
            .Produces<CostEstimate>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Calculates the cost estimate for a potential delivery.");

        deliveriesGroup.MapGet("/search", SearchDeliveries)
            .WithName("SearchDeliveries")
            .Produces<Page<DeliveryDto>>(StatusCodes.Status200OK)
            .WithDescription("Searches deliveries by cargo description for the currently authenticated customer.");
    }

    public static async Task<Created<DeliveryDto>> BookDelivery(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        IMapper<DeliveryRequestDto, DeliveryRequest> bookingRequestMapper,
        DeliveryRequestDto requestDto)
    {
        var request = bookingRequestMapper.Map(requestDto);
        var delivery = await deliveryService.Book(request);
        var deliveryDto = deliveryMapper.Map(delivery);
        return TypedResults.Created($"{ApiRoutes.Deliveries}/{deliveryDto.Id}", deliveryDto);
    }

    public static async Task<Results<Ok<DeliveryDto>, NotFound>> GetDelivery(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        Guid id)
    {
        var delivery = await deliveryService.GetById(id);
        if (delivery == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(deliveryMapper.Map(delivery));
    }

    public static async Task<Ok<Page<DeliveryDto>>> GetDeliveriesForCurrentCustomer(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var deliveries = await deliveryService.GetByCurrentCustomer(skip, take);
        var page = PageMapper.Map(deliveries, deliveryMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<Ok<Page<DeliveryDto>>> GetDeliveriesForCurrentPilot(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var deliveries = await deliveryService.GetByCurrentPilot(skip, take);
        var page = PageMapper.Map(deliveries, deliveryMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<Ok<Page<DeliveryDto>>> GetDeliveriesByPilot(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        Guid pilotId,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var deliveries = await deliveryService.GetByPilot(pilotId, skip, take);
        var page = PageMapper.Map(deliveries, deliveryMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<NoContent> CancelDelivery(
        IDeliveryService deliveryService,
        Guid id)
    {
        await deliveryService.Cancel(id);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> UpdateDeliveryStatus(
        IDeliveryService deliveryService,
        Guid id,
        UpdateDeliveryStatusRequest request)
    {
        await deliveryService.UpdateStatus(id, request.Status);
        return TypedResults.NoContent();
    }

    public static async Task<Ok<CostEstimate>> CalculateCost(
        IDeliveryService deliveryService,
        JobDetails details)
    {
        var estimate = await deliveryService.CalculateCost(details);
        return TypedResults.Ok(estimate);
    }

    public static async Task<Ok<Page<DeliveryDto>>> SearchDeliveries(
        IDeliveryService deliveryService,
        IMapper<Delivery, DeliveryDto> deliveryMapper,
        [FromQuery(Name = "searchTerm")] string searchTerm,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var deliveries = await deliveryService.SearchByCargoDescription(searchTerm, skip, take);
        var page = PageMapper.Map(deliveries, deliveryMapper);
        return TypedResults.Ok(page);
    }
}


