using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class PilotStore : IPilotStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<PilotStore> _logger;

    public PilotStore(MBCDbContext context, ILogger<PilotStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Pilot?> GetById(Guid id)
    {
        return await _context.Pilots.FindAsync(id);
    }

    public async Task<Page<Pilot>> GetPage(int skip, int take)
    {
        return await _context.Pilots.OrderBy(p => p.FullName).ToPageAsync(skip, take);
    }

    public async Task<Pilot> Create(Pilot pilot)
    {
        _context.Pilots.Add(pilot);
        await _context.SaveChangesAsync();
        return pilot;
    }

    public async Task<Pilot?> GetByUserId(Guid userId)
    {
        return await _context.Pilots.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}

