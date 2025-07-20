using MediatR;
using WorkflowPlatform.Domain.Common.Enumerations;

namespace WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Command to create a new workflow definition.
/// </summary>
public sealed record CreateWorkflowCommand : IRequest<CreateWorkflowResult>
{
    /// <summary>
    /// The name of the workflow
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the workflow
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The workflow definition (JSON or XML)
    /// </summary>
    public string? Definition { get; init; }

    /// <summary>
    /// The version of the workflow
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Whether the workflow is active
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// The workflow category
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Tags associated with the workflow
    /// </summary>
    public List<string>? Tags { get; init; }

    /// <summary>
    /// Priority level of the workflow
    /// </summary>
    public WorkflowPriority? Priority { get; init; }

    /// <summary>
    /// Default timeout for workflow execution
    /// </summary>
    public TimeSpan? DefaultTimeout { get; init; }

    /// <summary>
    /// Maximum number of concurrent executions allowed
    /// </summary>
    public int? MaxConcurrentExecutions { get; init; }

    /// <summary>
    /// Whether this workflow is a template
    /// </summary>
    public bool? IsTemplate { get; init; }

    /// <summary>
    /// Global variables for the workflow
    /// </summary>
    public Dictionary<string, object>? GlobalVariables { get; init; }
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
