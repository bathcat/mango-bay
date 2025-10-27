namespace MBC.Core.Entities;

/// <summary>
/// Represents the current status of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been initiated but not yet completed.
    /// </summary>
    Pending,

    /// <summary>
    /// Payment has been successfully processed.
    /// </summary>
    Completed,

    /// <summary>
    /// Payment has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Payment has been refunded to the customer.
    /// </summary>
    Refunded
}

