using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;

namespace MBC.Core.Persistence;

/// <summary>
/// Data store interface for Delivery entities.
/// </summary>
public interface IDeliveryStore : IStore<Guid, Delivery>
{
    /// <summary>
    /// Creates a new delivery booking.
    /// </summary>
    /// <param name="delivery">The delivery to create.</param>
    /// <returns>The created delivery.</returns>
    public Task<Delivery> Create(Delivery delivery);

    /// <summary>
    /// Updates an existing delivery.
    /// </summary>
    /// <param name="delivery">The delivery with updated values.</param>
    /// <returns>The updated delivery.</returns>
    public Task<Delivery> Update(Delivery delivery);

    /// <summary>
    /// Deletes a delivery by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public Task<bool> Delete(Guid id);

    /// <summary>
    /// Retrieves deliveries for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer's unique identifier.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paginated result of the customer's deliveries.</returns>
    public Task<Page<Delivery>> GetByCustomerId(Guid customerId, int skip, int take);

    /// <summary>
    /// Retrieves deliveries assigned to a specific pilot.
    /// </summary>
    /// <param name="pilotId">The pilot's unique identifier.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paginated result of the pilot's deliveries.</returns>
    public Task<Page<Delivery>> GetByPilotId(Guid pilotId, int skip, int take);

    /// <summary>
    /// Searches deliveries for a specific customer by cargo description.
    /// </summary>
    /// <param name="customerId">The customer's unique identifier.</param>
    /// <param name="searchTerm">The search term to match against cargo descriptions.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paginated result of matching deliveries.</returns>
    public Task<Page<Delivery>> SearchByCargoDescription(Guid customerId, string searchTerm, int skip, int take);
}

