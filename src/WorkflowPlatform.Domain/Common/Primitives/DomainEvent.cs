namespace WorkflowPlatform.Domain.Common.Primitives;

/// <summary>
/// Base class for all domain events.
/// Provides common properties and functionality for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the type name of the event for serialization purposes.
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Gets the aggregate identifier that this event relates to.
    /// </summary>
    public abstract Guid AggregateId { get; }

    /// <summary>
    /// Gets the version of the aggregate when this event occurred.
    /// </summary>
    public int AggregateVersion { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the DomainEvent class.
    /// </summary>
    /// <param name="aggregateVersion">The version of the aggregate when the event occurred</param>
    protected DomainEvent(int aggregateVersion = 1)
    {
        AggregateVersion = aggregateVersion;
    }
}
