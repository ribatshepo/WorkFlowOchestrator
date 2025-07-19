namespace WorkflowPlatform.Domain.Common.Primitives;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// Implements event sourcing pattern and domain event management.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Initializes a new instance of the AggregateRoot class.
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="createdBy">The user who created the aggregate</param>
    protected AggregateRoot(TId id, Guid createdBy) : base(id, createdBy)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AggregateRoot class.
    /// Used by EF Core for materialization.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Gets the domain events that have occurred within this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate.
    /// This should be called after events have been processed.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Updates the modification audit fields and raises a domain event if specified.
    /// </summary>
    /// <param name="modifiedBy">The user who modified the aggregate</param>
    /// <param name="domainEvent">Optional domain event to raise</param>
    protected void UpdateAggregate(Guid modifiedBy, IDomainEvent? domainEvent = null)
    {
        UpdateModified(modifiedBy);
        
        if (domainEvent != null)
        {
            RaiseDomainEvent(domainEvent);
        }
    }
}
