using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class DeliveryReviewStore : IDeliveryReviewStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<DeliveryReviewStore> _logger;

    public DeliveryReviewStore(MBCDbContext context, ILogger<DeliveryReviewStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DeliveryReview?> GetById(Guid id)
    {
        return await _context.DeliveryReviews
            // .Include(pr => pr.Pilot)
            // .Include(pr => pr.Customer)
            // .Include(pr => pr.Delivery)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<Page<DeliveryReview>> GetPage(int skip, int take)
    {
        var query = _context.DeliveryReviews
            // .Include(pr => pr.Pilot)
            // .Include(pr => pr.Customer)
            // .Include(pr => pr.Delivery)
            .OrderByDescending(r => r.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<DeliveryReview> Create(DeliveryReview review)
    {
        _context.DeliveryReviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<DeliveryReview> Update(DeliveryReview review)
    {
        var existing = await _context.DeliveryReviews.FindAsync(review.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"DeliveryReview with ID {review.Id} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(review);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> Delete(Guid id)
    {
        var review = await _context.DeliveryReviews.FindAsync(id);
        if (review == null)
        {
            return false;
        }

        _context.DeliveryReviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Page<DeliveryReview>> GetByPilotId(Guid pilotId, int skip, int take)
    {
        var query = _context.DeliveryReviews
            .Where(pr => pr.PilotId == pilotId)
            // .Include(pr => pr.Pilot)
            // .Include(pr => pr.Customer)
            // .Include(pr => pr.Delivery)
            .OrderByDescending(r => r.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<DeliveryReview?> GetByDeliveryId(Guid deliveryId)
    {
        return await _context.DeliveryReviews
            // .Include(pr => pr.Pilot)
            // .Include(pr => pr.Customer)
            // .Include(pr => pr.Delivery)
            .FirstOrDefaultAsync(pr => pr.DeliveryId == deliveryId);
    }
}


