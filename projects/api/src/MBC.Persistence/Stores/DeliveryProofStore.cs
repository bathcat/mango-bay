using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class DeliveryProofStore : IDeliveryProofStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<DeliveryProofStore> _logger;

    public DeliveryProofStore(MBCDbContext context, ILogger<DeliveryProofStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DeliveryProof?> GetById(Guid id)
    {
        return await _context.DeliveryProofs
            .Include(p => p.Delivery)
            .Include(p => p.Pilot)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Page<DeliveryProof>> GetPage(int skip, int take)
    {
        var query = _context.DeliveryProofs
            .Include(p => p.Delivery)
            .Include(p => p.Pilot)
            .Include(p => p.Customer)
            .OrderByDescending(p => p.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<DeliveryProof> Create(DeliveryProof proof)
    {
        _context.DeliveryProofs.Add(proof);
        await _context.SaveChangesAsync();
        return proof;
    }

    public async Task<DeliveryProof?> GetByDeliveryId(Guid deliveryId)
    {
        return await _context.DeliveryProofs
            .Include(p => p.Delivery)
            .Include(p => p.Pilot)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.DeliveryId == deliveryId);
    }

    public async Task<bool> Delete(Guid id)
    {
        var proof = await _context.DeliveryProofs.FindAsync(id);
        if (proof == null)
        {
            return false;
        }

        _context.DeliveryProofs.Remove(proof);
        await _context.SaveChangesAsync();
        return true;
    }
}

