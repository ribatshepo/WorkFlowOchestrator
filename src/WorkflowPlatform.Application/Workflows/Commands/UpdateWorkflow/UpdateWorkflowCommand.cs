using MediatR;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;

namespace WorkflowPlatform.Application.Workflows.Commands.UpdateWorkflow;

/// <summary>
/// Command to update an existing workflow definition.
/// </summary>
public sealed record UpdateWorkflowCommand : IRequest<UpdateWorkflowResult>
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public WorkflowDefinitionDto? Definition { get; init; }
    public Dictionary<string, object> GlobalVariables { get; init; } = new();
    public Dictionary<string, string> Metadata { get; init; } = new();
    public string LastModifiedBy { get; init; } = string.Empty;
    public bool IncrementVersion { get; init; } = true;
}

/// <summary>
/// Result of updating a workflow.
/// </summary>
public sealed record UpdateWorkflowResult
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NewVersion { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public IEnumerable<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public static UpdateWorkflowResult Success(Guid workflowId, string name, int newVersion, DateTime updatedAt)
        => new() { WorkflowId = workflowId, Name = name, NewVersion = newVersion, UpdatedAt = updatedAt, IsSuccess = true };

    public static UpdateWorkflowResult NotFound(Guid workflowId)
        => new() { WorkflowId = workflowId, IsSuccess = false, ErrorMessage = $"Workflow with ID {workflowId} was not found." };

    public static UpdateWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };

    public static UpdateWorkflowResult ValidationFailure(IEnumerable<string> validationErrors)
        => new() { IsSuccess = false, ValidationErrors = validationErrors };
}
