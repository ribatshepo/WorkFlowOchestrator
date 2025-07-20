using MediatR;
using WorkflowPlatform.Application.Common.Models;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;

namespace WorkflowPlatform.Application.Workflows.Queries.ListWorkflows;

/// <summary>
/// Query to list workflows with filtering, sorting, and pagination.
/// </summary>
public sealed record ListWorkflowsQuery : IRequest<PaginatedResult<ListWorkflowsResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
    public string? SortBy { get; init; } = "UpdatedAt";
    public bool SortDescending { get; init; } = true;

    public bool OrderDescending { get; init; } = true;
    public List<string>? Tags { get; init; }
    public bool IncludeDefinition { get; init; } = false;
}

/// <summary>
/// Result of listing workflows with pagination information.
/// </summary>
public sealed record ListWorkflowsResult
{
    public List<WorkflowSummaryDto> Workflows { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static ListWorkflowsResult Success(List<WorkflowSummaryDto> workflows, int totalCount, 
        int pageSize, int pageNumber)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new()
        {
            Workflows = workflows,
            TotalCount = totalCount,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1,
            IsSuccess = true
        };
    }

    public static ListWorkflowsResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Workflow summary data transfer object for list operations.
/// </summary>
public sealed record WorkflowSummaryDto
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
    public List<string> Tags { get; init; } = new();
    
    // Summary statistics
    public int NodeCount { get; init; }
    public int TotalExecutions { get; init; }
    public int SuccessfulExecutions { get; init; }
    public int FailedExecutions { get; init; }
    public DateTime? LastExecutedAt { get; init; }
    public TimeSpan? AverageExecutionTime { get; init; }
    
    // Optional detailed definition for when requested
    public WorkflowDefinitionDto? Definition { get; init; }
}

