using System;
using MBC.Core.Authorization;
using MBC.Core.Models;

namespace MBC.Core.Entities;

/// <summary>
/// Represents a cargo delivery booking (also referred to as a shipment).
/// </summary>
public sealed class Delivery : IPilotAssigned, ICustomerOwned
{
    /// <summary>
    /// Unique identifier for the delivery.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the customer who booked the delivery.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Foreign key reference to the assigned pilot.
    /// </summary>
    public Guid PilotId { get; set; }

    /// <summary>
    /// Foreign key reference to the associated payment.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Job details including origin, destination, cargo weight, and scheduled date.
    /// </summary>
    public JobDetails Details { get; set; } = new();

    /// <summary>
    /// Timestamp when the delivery was actually completed. Null if not yet delivered.
    /// </summary>
    public DateTime? CompletedOn { get; set; }

    /// <summary>
    /// Current status of the delivery.
    /// </summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the booking was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the booking was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property: The customer who booked this delivery.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property: The pilot assigned to this delivery.
    /// </summary>
    public Pilot? Pilot { get; set; }

    /// <summary>
    /// Navigation property: The origin site.
    /// </summary>
    public Site? Origin { get; set; }

    /// <summary>
    /// Navigation property: The destination site.
    /// </summary>
    public Site? Destination { get; set; }

    /// <summary>
    /// Navigation property: The payment for this delivery.
    /// </summary>
    public Payment? Payment { get; set; }

    /// <summary>
    /// Navigation property: Review for this delivery, if any.
    /// </summary>
    public DeliveryReview? Review { get; set; }

    /// <summary>
    /// Navigation property: Proof of delivery for this delivery, if any.
    /// </summary>
    public DeliveryProof? Proof { get; set; }
}

