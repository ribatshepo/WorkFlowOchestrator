using WorkflowPlatform.Domain.Common.Primitives;
using WorkflowPlatform.Domain.Common.Enumerations;

namespace WorkflowPlatform.Domain.Workflows.Events;

/// <summary>
/// Domain event raised when a new workflow definition is created.
/// </summary>
public record WorkflowCreatedEvent(
    Guid WorkflowId,
    string Name,
    string Description,
    Guid CreatedBy
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(WorkflowCreatedEvent);
    public Guid AggregateId => WorkflowId;
}

/// <summary>
/// Domain event raised when a workflow definition is updated.
/// </summary>
public record WorkflowUpdatedEvent(
    Guid WorkflowId,
    string Name,
    string Description,
    Guid UpdatedBy
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(WorkflowUpdatedEvent);
    public Guid AggregateId => WorkflowId;
}

/// <summary>
/// Domain event raised when a workflow status changes.
/// </summary>
public record WorkflowStatusChangedEvent(
    Guid WorkflowId,
    WorkflowStatus OldStatus,
    WorkflowStatus NewStatus,
    Guid ChangedBy
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(WorkflowStatusChangedEvent);
    public Guid AggregateId => WorkflowId;
}

/// <summary>
/// Domain event raised when a workflow is published.
/// </summary>
public record WorkflowPublishedEvent(
    Guid WorkflowId,
    string Name,
    Guid PublishedBy
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(WorkflowPublishedEvent);
    public Guid AggregateId => WorkflowId;
}

/// <summary>
/// Domain event raised when a workflow is archived.
/// </summary>
public record WorkflowArchivedEvent(
    Guid WorkflowId,
    string Name,
    Guid ArchivedBy,
    string Reason
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(WorkflowArchivedEvent);
    public Guid AggregateId => WorkflowId;
}
