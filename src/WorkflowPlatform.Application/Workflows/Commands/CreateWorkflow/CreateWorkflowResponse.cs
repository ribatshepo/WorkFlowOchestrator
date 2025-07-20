using System.ComponentModel.DataAnnotations;

namespace WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Response for creating a workflow
/// </summary>
public class CreateWorkflowResponse
{
    /// <summary>
    /// The unique identifier of the created workflow
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
}
