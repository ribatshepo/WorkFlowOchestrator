namespace WorkflowPlatform.Domain.Common.Primitives;

/// <summary>
/// Represents a domain event that has occurred within the domain.
/// Domain events are used to communicate between aggregates and bounded contexts.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the type name of the event for serialization purposes.
    /// </summary>
    string EventType { get; }
}
