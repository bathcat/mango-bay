using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class CustomerStore : ICustomerStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<CustomerStore> _logger;

    public CustomerStore(MBCDbContext context, ILogger<CustomerStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer?> GetById(Guid id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<Page<Customer>> GetPage(int skip, int take)
    {
        return await _context.Customers.OrderBy(c => c.Nickname).ToPageAsync(skip, take);
    }

    public async Task<Customer> Create(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> Update(Customer customer)
    {
        Customer? existing = await _context.Customers.FindAsync(customer.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Customer with ID {customer.Id} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(customer);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> Delete(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Customer?> GetByUserId(Guid userId)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}

