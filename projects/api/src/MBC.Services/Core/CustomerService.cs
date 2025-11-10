using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Core;

/// <summary>
/// Service for customer stuff. Pretty lightweight.
/// </summary>
/// <remarks>
/// Authorization strategy: imperative. It's easy to understand but:
/// * Mixes authorization with business logic,
/// * Isn't reusable, and
/// * Error-prone -- at least as the authorization + business logic get more complex.
///
/// Compare with DeliveryService which delegates to IMbcAuthorizationService
/// for separation of concerns.
/// </remarks>
public class CustomerService : ICustomerService
{
    private readonly ICustomerStore _customerStore;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CustomerService> _logger;

    private bool IsAdmin => _currentUser.User.IsInRole(UserRoles.Administrator);
    private bool IsCustomer => _currentUser.User.IsInRole(UserRoles.Customer);

    public CustomerService(
        ICustomerStore customerStore,
        ICurrentUser currentUser,
        ILogger<CustomerService> logger)
    {
        _customerStore = customerStore;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Customer> UpdateCustomer(Guid customerId, string nickname)
    {
        var isOwner = IsCustomer && _currentUser.CustomerId == customerId;

        if (!IsAdmin && !isOwner)
        {
            _logger.LogWarning(
                "User {UserId} attempted to update customer {CustomerId} without authorization",
                _currentUser.UserId,
                customerId);
            throw new UnauthorizedAccessException("You are not authorized to update this customer.");
        }

        _logger.LogInformation("Updating customer {CustomerId}", customerId);

        var customer = await _customerStore.GetById(customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", customerId);
            throw new InvalidOperationException($"Customer with ID {customerId} not found.");
        }

        customer.Nickname = nickname;

        var updatedCustomer = await _customerStore.Update(customer);

        _logger.LogInformation("Successfully updated customer {CustomerId}", customerId);

        return updatedCustomer;
    }

    public async Task<Customer?> GetCustomerById(Guid customerId)
    {
        var isOwner = IsCustomer && _currentUser.CustomerId == customerId;

        if (!IsAdmin && !isOwner)
        {
            _logger.LogWarning(
                "User {UserId} attempted to view customer {CustomerId} without authorization",
                _currentUser.UserId,
                customerId);
            throw new UnauthorizedAccessException("You are not authorized to view this customer.");
        }

        return await _customerStore.GetById(customerId);
    }

    public async Task<Customer?> GetCustomerByUserId(Guid userId)
    {
        var isOwner = IsCustomer && _currentUser.UserId == userId;

        if (!IsAdmin && !isOwner)
        {
            _logger.LogWarning(
                "User {UserId} attempted to view customer by user ID {TargetUserId} without authorization",
                _currentUser.UserId,
                userId);
            throw new UnauthorizedAccessException("You are not authorized to view this customer.");
        }

        return await _customerStore.GetByUserId(userId);
    }
}

