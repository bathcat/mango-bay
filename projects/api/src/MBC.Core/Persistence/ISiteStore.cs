using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

public interface ISiteStore : IStore<Guid, Site>
{
    public Task<Site> Create(Site site);

    public Task<Site> Update(Site site);

    public Task Delete(Guid id);

    public Task<bool> HasAssociatedDeliveries(Guid siteId);
}

