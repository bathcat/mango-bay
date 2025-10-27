using System.Threading.Tasks;
using MBC.Core.Models;


namespace MBC.Core.Persistence;

/// <summary>
/// Base interface for data stores providing read operations.
/// Specific stores should inherit from this and add their own create/update/delete methods as needed.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TEntity">The type of entity stored.</typeparam>
public interface IStore<TId, TEntity> where TEntity : class
{
    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    public Task<TEntity?> GetById(TId id);

    /// <summary>
    /// Retrieves a page of entities.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paginated result containing the requested entities.</returns>
    public Task<Page<TEntity>> GetPage(int skip, int take);
}

