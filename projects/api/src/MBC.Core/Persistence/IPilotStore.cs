using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

/// <summary>
/// Data store interface for Pilot entities.
/// </summary>
public interface IPilotStore : IStore<Guid, Pilot>
{
    /// <summary>
    /// Creates a new pilot profile.
    /// </summary>
    /// <param name="pilot">The pilot to create.</param>
    /// <returns>The created pilot.</returns>
    public Task<Pilot> Create(Pilot pilot);

    /// <summary>
    /// Retrieves a pilot by their associated user ID.
    /// </summary>
    /// <param name="userId">The ASP.NET Identity user identifier.</param>
    /// <returns>The pilot if found, otherwise null.</returns>
    public Task<Pilot?> GetByUserId(Guid userId);
}

