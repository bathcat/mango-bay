using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class PaymentStore : IPaymentStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<PaymentStore> _logger;

    public PaymentStore(MBCDbContext context, ILogger<PaymentStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Payment?> GetById(Guid id)
    {
        return await _context.Payments
            .Include(p => p.Delivery)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Page<Payment>> GetPage(int skip, int take)
    {
        var query = _context.Payments
            .Include(p => p.Delivery)
            .OrderByDescending(p => p.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<Payment> Create(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> Update(Payment payment)
    {
        var existing = await _context.Payments.FindAsync(payment.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Payment?> GetByDeliveryId(Guid deliveryId)
    {
        return await _context.Payments
            .Include(p => p.Delivery)
            .FirstOrDefaultAsync(p => p.DeliveryId == deliveryId);
    }
}

