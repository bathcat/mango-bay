using System;
using System.Threading.Tasks;
using MBC.Core;
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

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reviewsGroup = app.MapGroup(ApiRoutes.Reviews)
            .WithTags("Reviews");

        //TODO: Make non-anonymous.
        reviewsGroup.MapPost("/", CreateReview)
            .WithName("CreateReview")
            .Produces<DeliveryReviewDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Creates a new pilot review.");

        reviewsGroup.MapGet("/{id}", GetReview)
            .WithName("GetReview")
            .Produces<DeliveryReviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific review by its ID.");

        reviewsGroup.MapGet("/delivery/{deliveryId}", GetReviewByDelivery)
            .WithName("GetReviewByDelivery")
            .Produces<DeliveryReviewDto?>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a review for a specific delivery. Returns null if no review exists yet.");

        reviewsGroup.MapGet("/pilot/{pilotId}", GetReviewsByPilot)
            .WithName("GetReviewsByPilot")
            .Produces<Page<DeliveryReviewDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves paginated reviews for a specific pilot.");

        reviewsGroup.MapPut("/{reviewId}", UpdateReview)
            .WithName("UpdateReview")
            .Produces<DeliveryReviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates an existing review.");

        reviewsGroup.MapDelete("/{reviewId}", DeleteReview)
            .WithName("DeleteReview")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a review.");
    }

    public static async Task<Created<DeliveryReviewDto>> CreateReview(
        IReviewService reviewService,
        ICurrentUser currentUser,
        IMapper<DeliveryReview, DeliveryReviewDto> reviewMapper,
        CreateReviewRequest request)
    {
        var review = await reviewService.CreateReview(
            currentUser.CustomerId ?? throw new InvalidOperationException("Customer ID not found for current user"),
            request.DeliveryId,
            request.Rating,
            request.Notes);

        var reviewDto = reviewMapper.Map(review);
        return TypedResults.Created($"{ApiRoutes.Reviews}/{reviewDto.Id}", reviewDto);
    }

    public static async Task<Results<Ok<DeliveryReviewDto>, NotFound>> GetReview(
        IReviewService reviewService,
        IMapper<DeliveryReview, DeliveryReviewDto> reviewMapper,
        Guid id)
    {
        var review = await reviewService.GetReviewById(id);
        if (review == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(reviewMapper.Map(review));
    }

    public static async Task<Ok<DeliveryReviewDto?>> GetReviewByDelivery(
        IReviewService reviewService,
        IMapper<DeliveryReview, DeliveryReviewDto> reviewMapper,
        Guid deliveryId)
    {
        var review = await reviewService.GetReviewByDeliveryId(deliveryId);
        DeliveryReviewDto? result = reviewMapper.MapOptional(review);
        return TypedResults.Ok<DeliveryReviewDto?>(result);
    }

    public static async Task<Ok<Page<DeliveryReviewDto>>> GetReviewsByPilot(
        IReviewService reviewService,
        IMapper<DeliveryReview, DeliveryReviewDto> reviewMapper,
        Guid pilotId,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 5)
    {
        var reviews = await reviewService.GetByPilotId(pilotId, skip, take);
        var page = PageMapper.Map(reviews, reviewMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<Ok<DeliveryReviewDto>> UpdateReview(
        IReviewService reviewService,
        IMapper<DeliveryReview, DeliveryReviewDto> reviewMapper,
        Guid reviewId,
        UpdateReviewRequest request)
    {
        var review = await reviewService.UpdateReview(reviewId, request.Rating, request.Notes);
        var reviewDto = reviewMapper.Map(review);
        return TypedResults.Ok(reviewDto);
    }

    public static async Task<NoContent> DeleteReview(
        IReviewService reviewService,
        Guid reviewId)
    {
        await reviewService.DeleteReview(reviewId);
        return TypedResults.NoContent();
    }
}
