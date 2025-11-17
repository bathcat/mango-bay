using System;
using MBC.Core.Authorization;
using MBC.Core.ValueObjects;

namespace MBC.Core.Entities;

/// <summary>
/// Represents a customer review and rating of a pilot for a completed delivery.
/// </summary>
public sealed class DeliveryReview : IPilotAssigned, ICustomerOwned
{
    /// <summary>
    /// Unique identifier for the review.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the pilot being reviewed.
    /// </summary>
    public Guid PilotId { get; set; }

    /// <summary>
    /// Foreign key reference to the customer who wrote the review.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Foreign key reference to the associated completed delivery.
    /// </summary>
    public Guid DeliveryId { get; set; }

    /// <summary>
    /// Star rating from 1 to 5.
    /// </summary>
    public Rating Rating { get; set; }

    /// <summary>
    /// Rich text HTML content of the review (from Quill editor).
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the review was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

}


