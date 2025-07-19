using MediatR;
using WorkflowPlatform.Domain.Common.Enumerations;

namespace WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Command to create a new workflow definition.
/// </summary>
public sealed record CreateWorkflowCommand : IRequest<CreateWorkflowResult>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = "General";
    public WorkflowPriority Priority { get; init; } = WorkflowPriority.Normal;
    public TimeSpan? DefaultTimeout { get; init; }
    public int MaxConcurrentExecutions { get; init; } = 10;
    public bool IsTemplate { get; init; } = false;
    public Dictionary<string, object> GlobalVariables { get; init; } = new();
}

/// <summary>
/// Result of creating a new workflow.
/// </summary>
public sealed record CreateWorkflowResult
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public IEnumerable<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public static CreateWorkflowResult Success(Guid workflowId, string name)
        => new() { WorkflowId = workflowId, Name = name, IsSuccess = true };

    public static CreateWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };

    public static CreateWorkflowResult ValidationFailure(IEnumerable<string> validationErrors)
        => new() { IsSuccess = false, ValidationErrors = validationErrors };
}
