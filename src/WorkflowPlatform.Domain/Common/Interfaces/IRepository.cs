using WorkflowPlatform.Domain.Common.Primitives;

namespace WorkflowPlatform.Domain.Common.Interfaces;

/// <summary>
/// Represents a generic repository pattern interface for aggregate roots.
/// Provides basic CRUD operations with async support.
/// This interface belongs in the Domain layer as it defines the contract
/// that the domain needs for persistence operations.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type</typeparam>
/// <typeparam name="TId">The identifier type</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Gets an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The aggregate if found, null otherwise</returns>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added aggregate</returns>
    Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated aggregate</returns>
    Task<TAggregate> UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an aggregate exists with the specified identifier.
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the aggregate exists, false otherwise</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the data store</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
