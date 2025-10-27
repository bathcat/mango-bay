using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Core;

//TODO: Add authorization.
public class CustomerService : ICustomerService
{
    private readonly ICustomerStore _customerStore;
    private readonly UserManager<MBCUser> _userManager;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerStore customerStore,
        UserManager<MBCUser> userManager,
        ILogger<CustomerService> logger)
    {
        _customerStore = customerStore;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Customer> CreateCustomer(CreateCustomerRequest request)
    {
        _logger.LogInformation("Creating customer account for {Username}", request.Username);

        var user = new MBCUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Username}: {Errors}", request.Username, errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        _logger.LogDebug("User {UserId} created, assigning Customer role", user.Id);

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Customer role to user {UserId}, rolling back", user.Id);
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role: {errors}");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Nickname = request.Nickname
        };

        var createdCustomer = await _customerStore.Create(customer);

        _logger.LogInformation(
            "Successfully created customer {CustomerId} with user {UserId}",
            createdCustomer.Id,
            user.Id);

        return createdCustomer;
    }

    public async Task<Customer> UpdateCustomer(Guid customerId, string nickname)
    {
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
        return await _customerStore.GetById(customerId);
    }

    public async Task<Customer?> GetCustomerByUserId(Guid userId)
    {
        return await _customerStore.GetByUserId(userId);
    }
}

