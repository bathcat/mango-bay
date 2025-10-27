using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;

namespace MBC.Core.Persistence;

/// <summary>
/// Data store interface for DeliveryReview entities.
/// </summary>
public interface IDeliveryReviewStore : IStore<Guid, DeliveryReview>
{
    /// <summary>
    /// Creates a new pilot review.
    /// </summary>
    /// <param name="review">The review to create.</param>
    /// <returns>The created review.</returns>
    public Task<DeliveryReview> Create(DeliveryReview review);

    /// <summary>
    /// Updates an existing pilot review.
    /// </summary>
    /// <param name="review">The review with updated values.</param>
    /// <returns>The updated review.</returns>
    public Task<DeliveryReview> Update(DeliveryReview review);

    /// <summary>
    /// Deletes a review by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the review to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public Task<bool> Delete(Guid id);

    /// <summary>
    /// Retrieves reviews for a specific pilot.
    /// </summary>
    /// <param name="pilotId">The pilot's unique identifier.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paginated result of the pilot's reviews.</returns>
    public Task<Page<DeliveryReview>> GetByPilotId(Guid pilotId, int skip, int take);

    /// <summary>
    /// Retrieves a review for a specific delivery.
    /// </summary>
    /// <param name="deliveryId">The delivery's unique identifier.</param>
    /// <returns>The review if found, otherwise null.</returns>
    public Task<DeliveryReview?> GetByDeliveryId(Guid deliveryId);
}


