using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class SiteStore : ISiteStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<SiteStore> _logger;

    public SiteStore(MBCDbContext context, ILogger<SiteStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Site?> GetById(Guid id)
    {
        return await _context.Sites.FindAsync(id);
    }

    public async Task<Page<Site>> GetPage(int skip, int take)
    {
        return await _context.Sites.OrderBy(s => s.Name).ToPageAsync(skip, take);
    }

    public async Task<Site> Create(Site site)
    {
        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task<Site> Update(Site site)
    {
        _context.Sites.Update(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task Delete(Guid id)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site != null)
        {
            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasAssociatedDeliveries(Guid siteId)
    {
        return await _context.Deliveries
            .AnyAsync(d => d.Details.OriginId == siteId || d.Details.DestinationId == siteId);
    }
}

