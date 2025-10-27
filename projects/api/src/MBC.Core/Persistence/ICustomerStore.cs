using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

/// <summary>
/// Data store interface for Customer entities.
/// </summary>
public interface ICustomerStore : IStore<Guid, Customer>
{
    /// <summary>
    /// Creates a new customer profile.
    /// </summary>
    /// <param name="customer">The customer to create.</param>
    /// <returns>The created customer.</returns>
    public Task<Customer> Create(Customer customer);

    /// <summary>
    /// Updates an existing customer profile.
    /// </summary>
    /// <param name="customer">The customer with updated values.</param>
    /// <returns>The updated customer.</returns>
    public Task<Customer> Update(Customer customer);

    /// <summary>
    /// Deletes a customer by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public Task<bool> Delete(Guid id);

    /// <summary>
    /// Retrieves a customer by their associated user ID.
    /// </summary>
    /// <param name="userId">The ASP.NET Identity user identifier.</param>
    /// <returns>The customer if found, otherwise null.</returns>
    public Task<Customer?> GetByUserId(Guid userId);
}

