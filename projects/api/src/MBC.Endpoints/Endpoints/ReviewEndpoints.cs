using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reviewsGroup = app.MapGroup(ApiRoutes.Reviews)
            .WithTags("Reviews");


        reviewsGroup.MapPost("/", CreateReview)
            .WithName("CreateReview")
            .RequireAuthorization()
            .Produces<DeliveryReview>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Creates a new pilot review.");

        reviewsGroup.MapGet("/{id}", GetReview)
            .WithName("GetReview")
            .Produces<DeliveryReview>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific review by its ID.");

        reviewsGroup.MapGet("/delivery/{deliveryId}", GetReviewByDelivery)
            .WithName("GetReviewByDelivery")
            .RequireAuthorization()
            .Produces<DeliveryReview?>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a review for a specific delivery. Returns null if no review exists yet.");

        reviewsGroup.MapGet("/pilot/{pilotId}", GetReviewsByPilot)
            .WithName("GetReviewsByPilot")
            .Produces<Page<DeliveryReview>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves paginated reviews for a specific pilot.");

        reviewsGroup.MapPut("/{reviewId}", UpdateReview)
            .WithName("UpdateReview")
            .RequireAuthorization()
            .Produces<DeliveryReview>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates an existing review.");

        reviewsGroup.MapDelete("/{reviewId}", DeleteReview)
            .WithName("DeleteReview")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a review.");
    }

    public static async Task<Created<DeliveryReview>> CreateReview(
        IReviewService reviewService,
        ICurrentUser currentUser,
        CreateReviewRequest request)
    {
        var review = await reviewService.CreateReview(
            currentUser.CustomerId ?? throw new InvalidOperationException("Customer ID not found for current user"),
            request.DeliveryId,
            request.Rating,
            request.Notes);

        return TypedResults.Created($"{ApiRoutes.Reviews}/{review.Id}", review);
    }

    public static async Task<Results<Ok<DeliveryReview>, NotFound>> GetReview(
        IReviewService reviewService,
        Guid id)
    {
        var review = await reviewService.GetReviewById(id);
        if (review == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(review);
    }

    public static async Task<Ok<DeliveryReview?>> GetReviewByDelivery(
        IReviewService reviewService,
        Guid deliveryId)
    {
        var review = await reviewService.GetReviewByDeliveryId(deliveryId);
        return TypedResults.Ok<DeliveryReview?>(review);
    }

    public static async Task<Ok<Page<DeliveryReview>>> GetReviewsByPilot(
        IReviewService reviewService,
        Guid pilotId,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 5)
    {
        var reviews = await reviewService.GetByPilotId(pilotId, skip, take);
        return TypedResults.Ok(reviews);
    }

    public static async Task<Ok<DeliveryReview>> UpdateReview(
        IReviewService reviewService,
        Guid reviewId,
        UpdateReviewRequest request)
    {
        var review = await reviewService.UpdateReview(reviewId, request.Rating, request.Notes);
        return TypedResults.Ok(review);
    }

    public static async Task<NoContent> DeleteReview(
        IReviewService reviewService,
        Guid reviewId)
    {
        await reviewService.DeleteReview(reviewId);
        return TypedResults.NoContent();
    }
}
