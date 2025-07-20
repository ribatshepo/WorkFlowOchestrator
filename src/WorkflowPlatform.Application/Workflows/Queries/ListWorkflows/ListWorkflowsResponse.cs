namespace WorkflowPlatform.Application.Workflows.Queries.ListWorkflows;

/// <summary>
/// Response for listing workflows
/// </summary>
public class ListWorkflowsResponse
{
    /// <summary>
    /// The unique identifier of the workflow
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the workflow
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the workflow
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The version of the workflow
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the workflow is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The workflow category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// The workflow tags
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The date and time when the workflow was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the workflow was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The number of times this workflow has been executed
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// The date and time when the workflow was last executed
    /// </summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>
    /// The average execution time in milliseconds
    /// </summary>
    public double? AverageExecutionTime { get; set; }
}
