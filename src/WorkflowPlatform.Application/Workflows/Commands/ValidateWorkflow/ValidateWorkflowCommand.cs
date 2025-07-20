using MediatR;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;

namespace WorkflowPlatform.Application.Workflows.Commands.ValidateWorkflow;

/// <summary>
/// Command to validate a workflow definition without saving it.
/// </summary>
public sealed record ValidateWorkflowCommand : IRequest<ValidateWorkflowResult>
{
    public WorkflowDefinitionDto Definition { get; init; } = new();
    public bool StrictValidation { get; init; } = false;
    public bool CheckNodeCompatibility { get; init; } = true;
    public bool ValidateConnections { get; init; } = true;
    public bool CheckCircularDependencies { get; init; } = true;
    public Dictionary<string, object>? TestInputData { get; init; }
}

/// <summary>
/// Result of workflow validation.
/// </summary>
public sealed record ValidateWorkflowResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
    public List<ValidationWarning> Warnings { get; init; } = new();
    public List<ValidationSuggestion> Suggestions { get; init; } = new();
    public ValidationSummary Summary { get; init; } = new();
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static ValidateWorkflowResult Success(bool isValid, List<ValidationError> errors, 
        List<ValidationWarning> warnings, List<ValidationSuggestion> suggestions, ValidationSummary summary)
        => new()
        {
            IsValid = isValid,
            Errors = errors,
            Warnings = warnings,
            Suggestions = suggestions,
            Summary = summary,
            IsSuccess = true
        };

    public static ValidateWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Validation error details.
/// </summary>
public sealed record ValidationError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "Error";
    public string? NodeId { get; init; }
    public string? PropertyPath { get; init; }
    public Dictionary<string, object> Context { get; init; } = new();
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Validation warning details.
/// </summary>
public sealed record ValidationWarning
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? NodeId { get; init; }
    public string? PropertyPath { get; init; }
    public Dictionary<string, object> Context { get; init; } = new();
    public string? Recommendation { get; init; }
}

/// <summary>
/// Validation suggestion details.
/// </summary>
public sealed record ValidationSuggestion
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? NodeId { get; init; }
    public string Category { get; init; } = string.Empty;
    public int Priority { get; init; } = 0; // Higher number = higher priority
    public Dictionary<string, object> Context { get; init; } = new();
}

/// <summary>
/// Validation summary statistics.
/// </summary>
public sealed record ValidationSummary
{
    public int TotalNodes { get; init; }
    public int TotalConnections { get; init; }
    public int ValidNodes { get; init; }
    public int InvalidNodes { get; init; }
    public int OrphanNodes { get; init; }
    public int CircularDependencies { get; init; }
    public int UnreachableNodes { get; init; }
    public bool HasStartNode { get; init; }
    public bool HasEndNode { get; init; }
    public int MaxDepth { get; init; }
    public Dictionary<string, int> NodeTypeCount { get; init; } = new();
    public TimeSpan EstimatedExecutionTime { get; init; }
}
