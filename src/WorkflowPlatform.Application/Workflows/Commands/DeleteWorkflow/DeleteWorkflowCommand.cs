using MediatR;

namespace WorkflowPlatform.Application.Workflows.Commands.DeleteWorkflow;

/// <summary>
/// Command to delete a workflow and optionally all related data.
/// </summary>
public sealed record DeleteWorkflowCommand : IRequest<DeleteWorkflowResult>
{
    public Guid WorkflowId { get; init; }
    public bool ForceDelete { get; init; } = false;
    public string DeletedBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public bool DeleteExecutionHistory { get; init; } = false;
}

/// <summary>
/// Result of deleting a workflow.
/// </summary>
public sealed record DeleteWorkflowResult
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int RelatedRecordsDeleted { get; init; }

    public static DeleteWorkflowResult Success(Guid workflowId, string name, DateTime deletedAt, int relatedRecordsDeleted = 0)
        => new() { WorkflowId = workflowId, Name = name, DeletedAt = deletedAt, RelatedRecordsDeleted = relatedRecordsDeleted, IsSuccess = true };

    public static DeleteWorkflowResult NotFound(Guid workflowId)
        => new() { WorkflowId = workflowId, IsSuccess = false, ErrorMessage = $"Workflow with ID {workflowId} was not found." };

    public static DeleteWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };

    public static DeleteWorkflowResult CannotDelete(Guid workflowId, string reason)
        => new() { WorkflowId = workflowId, IsSuccess = false, ErrorMessage = $"Cannot delete workflow: {reason}" };
}
