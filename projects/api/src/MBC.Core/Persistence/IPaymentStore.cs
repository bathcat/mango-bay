using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

/// <summary>
/// Data store interface for Payment entities.
/// </summary>
public interface IPaymentStore : IStore<Guid, Payment>
{
    /// <summary>
    /// Creates a new payment record.
    /// </summary>
    /// <param name="payment">The payment to create.</param>
    /// <returns>The created payment.</returns>
    public Task<Payment> Create(Payment payment);

    /// <summary>
    /// Updates an existing payment record.
    /// </summary>
    /// <param name="payment">The payment with updated values.</param>
    /// <returns>The updated payment.</returns>
    public Task<Payment> Update(Payment payment);

    /// <summary>
    /// Retrieves a payment by its associated delivery ID.
    /// </summary>
    /// <param name="deliveryId">The delivery's unique identifier.</param>
    /// <returns>The payment if found, otherwise null.</returns>
    public Task<Payment?> GetByDeliveryId(Guid deliveryId);
}

