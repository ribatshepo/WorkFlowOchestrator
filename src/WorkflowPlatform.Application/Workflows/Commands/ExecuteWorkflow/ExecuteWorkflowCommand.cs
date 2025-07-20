using MediatR;

namespace WorkflowPlatform.Application.Workflows.Commands.ExecuteWorkflow;

/// <summary>
/// Command to execute a workflow with specified input data and context.
/// </summary>
public sealed record ExecuteWorkflowCommand : IRequest<ExecuteWorkflowResult>
{
    public Guid WorkflowId { get; init; }
    public string InputData { get; init; } = "{}"; // JSON string
    public Dictionary<string, string> ExecutionContext { get; init; } = new();
    public TimeSpan? Timeout { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public int Priority { get; init; } = 0;
    public bool EnableRealTimeUpdates { get; init; } = true;
    public List<string>? NotificationChannels { get; init; }
}

/// <summary>
/// Result of workflow execution initiation.
/// </summary>
public sealed record ExecuteWorkflowResult
{
    public Guid ExecutionId { get; init; }
    public Guid WorkflowId { get; init; }
    public string WorkflowName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public TimeSpan? EstimatedDuration { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object> ExecutionMetadata { get; init; } = new();

    public static ExecuteWorkflowResult Success(Guid executionId, Guid workflowId, string workflowName, 
        DateTime startedAt, TimeSpan? estimatedDuration = null, Dictionary<string, object>? metadata = null)
        => new()
        {
            ExecutionId = executionId,
            WorkflowId = workflowId,
            WorkflowName = workflowName,
            Status = "Running",
            StartedAt = startedAt,
            EstimatedDuration = estimatedDuration,
            ExecutionMetadata = metadata ?? new(),
            IsSuccess = true
        };

    public static ExecuteWorkflowResult WorkflowNotFound(Guid workflowId)
        => new() { WorkflowId = workflowId, IsSuccess = false, ErrorMessage = $"Workflow with ID {workflowId} was not found." };

    public static ExecuteWorkflowResult WorkflowInactive(Guid workflowId, string workflowName)
        => new() 
        { 
            WorkflowId = workflowId, 
            WorkflowName = workflowName, 
            IsSuccess = false, 
            ErrorMessage = "Workflow is not in an active state and cannot be executed." 
        };

    public static ExecuteWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };

    public static ExecuteWorkflowResult ValidationFailure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = $"Validation failed: {errorMessage}" };
}
