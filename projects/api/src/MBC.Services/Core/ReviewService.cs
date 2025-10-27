using System;
using System.Threading.Tasks;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Core;

/// <summary>
/// Service for managing pilot reviews with resource-based authorization.
/// </summary>
/// <remarks>
/// This service handles entity-level authorization (can user access this review?), but clients are
/// responsible for field-level security (which fields should be exposed?).
///
/// For example, DeliveryReview entities contain CustomerId, but the HTTP API uses DeliveryReviewMapper
/// to produce DeliveryReviewDto, which omits CustomerId to prevent exposure in the public API.
///
/// This separation ensures authorization logic is portable across different client types (HTTP, CLI, WebSockets)
/// while allowing each client to shape data appropriately for its context.
/// </remarks>
public class ReviewService : IReviewService
{
    private readonly IDeliveryReviewStore _reviewStore;
    private readonly IDeliveryStore _deliveryStore;
    private readonly IMbcAuthorizationService _authorizationService;
    private readonly ILogger<ReviewService> _logger;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public ReviewService(
        IDeliveryReviewStore reviewStore,
        IDeliveryStore deliveryStore,
        IMbcAuthorizationService authorizationService,
        ILogger<ReviewService> logger,
        IHtmlSanitizer htmlSanitizer)
    {
        _reviewStore = reviewStore;
        _deliveryStore = deliveryStore;
        _authorizationService = authorizationService;
        _logger = logger;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<DeliveryReview> CreateReview(Guid customerId, Guid deliveryId, Rating rating, string notes)
    {
        _logger.LogInformation(
            "Creating review for delivery {DeliveryId} by customer {CustomerId}",
            deliveryId,
            customerId);

        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(ReviewOperations.Create, delivery);

        if (delivery.Status != DeliveryStatus.Delivered)
        {
            _logger.LogWarning(
                "Cannot review delivery {DeliveryId} with status {Status}",
                deliveryId,
                delivery.Status);
            throw new InvalidOperationException($"Cannot review delivery {deliveryId} that is not yet delivered.");
        }

        var existingReview = await _reviewStore.GetByDeliveryId(deliveryId);
        if (existingReview != null)
        {
            _logger.LogWarning("Delivery {DeliveryId} has already been reviewed", deliveryId);
            throw new InvalidOperationException($"Delivery {deliveryId} has already been reviewed.");
        }

        var sanitizedNotes = _htmlSanitizer.Sanitize(notes);

        var review = new DeliveryReview
        {
            Id = Guid.NewGuid(),
            PilotId = delivery.PilotId,
            CustomerId = customerId,
            DeliveryId = deliveryId,
            Rating = rating,
            Notes = sanitizedNotes,
            CreatedAt = DateTime.UtcNow
        };

        var createdReview = await _reviewStore.Create(review);

        _logger.LogInformation(
            "Successfully created review {ReviewId} for pilot {PilotId} with rating {Rating}",
            createdReview.Id,
            delivery.PilotId,
            rating);

        return createdReview;
    }

    public async Task<DeliveryReview> UpdateReview(Guid reviewId, Rating rating, string notes)
    {
        _logger.LogInformation("Updating review {ReviewId}", reviewId);

        var review = await _reviewStore.GetById(reviewId);
        if (review == null)
        {
            _logger.LogWarning("Review {ReviewId} not found", reviewId);
            throw new InvalidOperationException($"Review with ID {reviewId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(ReviewOperations.Update, review);

        var sanitizedNotes = _htmlSanitizer.Sanitize(notes);

        review.Rating = rating;
        review.Notes = sanitizedNotes;

        var updatedReview = await _reviewStore.Update(review);

        _logger.LogInformation("Successfully updated review {ReviewId}", reviewId);

        return updatedReview;
    }

    public async Task DeleteReview(Guid reviewId)
    {
        _logger.LogInformation("Deleting review {ReviewId}", reviewId);

        var review = await _reviewStore.GetById(reviewId);
        if (review == null)
        {
            _logger.LogWarning("Review {ReviewId} not found", reviewId);
            throw new InvalidOperationException($"Review with ID {reviewId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(ReviewOperations.Delete, review);

        var deleted = await _reviewStore.Delete(reviewId);
        if (!deleted)
        {
            _logger.LogWarning("Failed to delete review {ReviewId}", reviewId);
            throw new InvalidOperationException($"Failed to delete review {reviewId}.");
        }

        _logger.LogInformation("Successfully deleted review {ReviewId}", reviewId);
    }



    public async Task<DeliveryReview?> GetReviewById(Guid reviewId)
    {
        return await _reviewStore.GetById(reviewId);
    }

    /// <summary>
    /// Retrieves a review by its associated delivery ID.
    /// </summary>
    /// <remarks>
    /// Context-dependent authorization: This method checks authorization on the Delivery, not the Review.
    /// While reviews are publicly readable (anonymous users can view them), accessing a review via deliveryId
    /// requires delivery authorization because the deliveryId itself is sensitive information.
    ///
    /// Returns null for both "delivery doesn't exist" and "not authorized to view delivery" to prevent
    /// information disclosure. Returning 401 for unauthorized access would confirm the delivery exists,
    /// enabling enumeration attacks to discover which delivery IDs are valid.
    ///
    /// This demonstrates two security principles:
    /// 1. Authorization requirements can vary based on the access path to the same resource
    /// 2. Error responses must be carefully designed to avoid leaking information about resource existence
    /// </remarks>
    public async Task<DeliveryReview?> GetReviewByDeliveryId(Guid deliveryId)
    {
        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery == null)
        {
            return null;
        }

        try
        {
            _authorizationService.ThrowIfUnauthorized(DeliveryOperations.Read, delivery);
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return await _reviewStore.GetByDeliveryId(deliveryId);
    }

    public async Task<Page<DeliveryReview>> GetByPilotId(Guid pilotId, int skip, int take)
    {
        _logger.LogInformation(
            "Retrieving reviews for pilot {PilotId} with pagination (skip: {Skip}, take: {Take})",
            pilotId,
            skip,
            take);

        return await _reviewStore.GetByPilotId(pilotId, skip, take);
    }
}

