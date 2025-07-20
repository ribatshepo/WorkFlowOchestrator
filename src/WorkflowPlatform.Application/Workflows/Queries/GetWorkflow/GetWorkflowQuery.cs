using MediatR;

namespace WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;

/// <summary>
/// Query to get a workflow by ID with optional inclusion of definition details.
/// </summary>
public sealed record GetWorkflowQuery : IRequest<GetWorkflowResult>
{
    public Guid Id { get; init; }
    public bool IncludeDefinition { get; init; } = true;
    public bool IncludeExecutionHistory { get; init; } = false;
    public bool IncludeMetrics { get; init; } = false;
}

/// <summary>
/// Result of getting a workflow.
/// </summary>
public sealed record GetWorkflowResult
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string LastModifiedBy { get; init; } = string.Empty;
    public int Version { get; init; }
    public bool IsTemplate { get; init; }
    public Dictionary<string, object> GlobalVariables { get; init; } = new();
    public Dictionary<string, string> Metadata { get; init; } = new();
    
    // Optional detailed information
    public WorkflowDefinitionDto? Definition { get; init; }
    public List<ExecutionHistoryDto>? ExecutionHistory { get; init; }
    public WorkflowMetricsDto? Metrics { get; init; }
    
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static GetWorkflowResult Success(Guid workflowId, string name, string description, 
        string category, string status, DateTime createdAt, DateTime updatedAt, 
        string createdBy, string lastModifiedBy, int version, bool isTemplate,
        Dictionary<string, object> globalVariables, Dictionary<string, string> metadata,
        WorkflowDefinitionDto? definition = null, List<ExecutionHistoryDto>? executionHistory = null,
        WorkflowMetricsDto? metrics = null)
        => new()
        {
            WorkflowId = workflowId,
            Name = name,
            Description = description,
            Category = category,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            LastModifiedBy = lastModifiedBy,
            Version = version,
            IsTemplate = isTemplate,
            GlobalVariables = globalVariables,
            Metadata = metadata,
            Definition = definition,
            ExecutionHistory = executionHistory,
            Metrics = metrics,
            IsSuccess = true
        };

    public static GetWorkflowResult NotFound(Guid workflowId)
        => new() { WorkflowId = workflowId, IsSuccess = false, ErrorMessage = $"Workflow with ID {workflowId} was not found." };

    public static GetWorkflowResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Workflow definition data transfer object.
/// </summary>
public sealed record WorkflowDefinitionDto
{
    public List<WorkflowNodeDto> Nodes { get; init; } = new();
    public List<WorkflowEdgeDto> Edges { get; init; } = new();
    public WorkflowSettingsDto Settings { get; init; } = new();
}

/// <summary>
/// Workflow node data transfer object.
/// </summary>
public sealed record WorkflowNodeDto
{
    public string NodeId { get; init; } = string.Empty;
    public string NodeType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, object> Configuration { get; init; } = new();
    public PositionDto Position { get; init; } = new();
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Workflow edge data transfer object.
/// </summary>
public sealed record WorkflowEdgeDto
{
    public string EdgeId { get; init; } = string.Empty;
    public string SourceNodeId { get; init; } = string.Empty;
    public string TargetNodeId { get; init; } = string.Empty;
    public string SourceHandle { get; init; } = string.Empty;
    public string TargetHandle { get; init; } = string.Empty;
    public EdgeConditionDto? Condition { get; init; }
}

/// <summary>
/// Position data transfer object.
/// </summary>
public sealed record PositionDto
{
    public double X { get; init; }
    public double Y { get; init; }
}

/// <summary>
/// Edge condition data transfer object.
/// </summary>
public sealed record EdgeConditionDto
{
    public string ConditionType { get; init; } = string.Empty;
    public string Expression { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
}

/// <summary>
/// Workflow settings data transfer object.
/// </summary>
public sealed record WorkflowSettingsDto
{
    public TimeSpan MaxExecutionTime { get; init; } = TimeSpan.FromMinutes(30);
    public int MaxParallelNodes { get; init; } = 10;
    public bool EnableLogging { get; init; } = true;
    public string LogLevel { get; init; } = "Information";
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();
}

/// <summary>
/// Execution history data transfer object.
/// </summary>
public sealed record ExecutionHistoryDto
{
    public Guid ExecutionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
    public int NodesExecuted { get; init; }
    public int NodesSucceeded { get; init; }
    public int NodesFailed { get; init; }
}

/// <summary>
/// Workflow metrics data transfer object.
/// </summary>
public sealed record WorkflowMetricsDto
{
    public int TotalExecutions { get; init; }
    public int SuccessfulExecutions { get; init; }
    public int FailedExecutions { get; init; }
    public double SuccessRate { get; init; }
    public TimeSpan AverageExecutionTime { get; init; }
    public DateTime LastExecutedAt { get; init; }
    public Dictionary<string, object> CustomMetrics { get; init; } = new();
}
