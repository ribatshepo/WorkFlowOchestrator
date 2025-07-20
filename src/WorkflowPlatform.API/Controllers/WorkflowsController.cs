using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.DeleteWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.UpdateWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.ValidateWorkflow;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;
using WorkflowPlatform.Application.Workflows.Queries.ListWorkflows;

namespace WorkflowPlatform.API.Controllers;

/// <summary>
/// Controller for workflow management operations (CRUD).
/// Provides REST API endpoints for workflow lifecycle management.
/// </summary>
[Authorize]
[EnableRateLimiting("WorkflowPolicy")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflows")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
public class WorkflowsController : ApiControllerBase
{
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(ILogger<WorkflowsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new workflow definition.
    /// </summary>
    /// <param name="request">Workflow creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workflow information</returns>
    /// <response code="201">Workflow created successfully</response>
    /// <response code="400">Invalid workflow definition or validation errors</response>
    /// <response code="409">Workflow with the same name already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateWorkflowResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateWorkflow(
        [FromBody] CreateWorkflowRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateWorkflowCommand
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category ?? "General",
                Priority = Enum.TryParse<WorkflowPlatform.Domain.Common.Enumerations.WorkflowPriority>(
                    request.Priority, out var priority) ? priority : 
                    WorkflowPlatform.Domain.Common.Enumerations.WorkflowPriority.Normal,
                DefaultTimeout = request.TimeoutMinutes.HasValue ? TimeSpan.FromMinutes(request.TimeoutMinutes.Value) : null,
                MaxConcurrentExecutions = request.MaxConcurrentExecutions ?? 10,
                IsTemplate = request.IsTemplate ?? false,
                GlobalVariables = request.GlobalVariables ?? new Dictionary<string, object>()
            };

            var result = await Mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    var problemDetails = new ValidationProblemDetails();
                    foreach (var error in result.ValidationErrors)
                    {
                        problemDetails.Errors.Add("ValidationError", new[] { error });
                    }
                    return BadRequest(problemDetails);
                }
                return BadRequest(new ProblemDetails { Title = "Workflow Creation Failed", Detail = result.ErrorMessage });
            }

            var response = new CreateWorkflowResponseDto
            {
                WorkflowId = result.WorkflowId,
                Name = result.Name,
                CreatedAt = DateTime.UtcNow,
                Version = 1
            };

            _logger.LogInformation("Workflow {WorkflowId} '{Name}' created successfully", result.WorkflowId, result.Name);

            return CreatedAtAction(
                nameof(GetWorkflow), 
                new { id = result.WorkflowId }, 
                CreateSuccessResponse(response, "Workflow created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow: {Name}", request.Name);
            return CreateErrorResponse("An error occurred while creating the workflow", ex.Message);
        }
    }

    /// <summary>
    /// Gets a workflow by ID with optional detailed information.
    /// </summary>
    /// <param name="id">Workflow unique identifier</param>
    /// <param name="includeDefinition">Include workflow definition details</param>
    /// <param name="includeHistory">Include execution history</param>
    /// <param name="includeMetrics">Include workflow metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow details</returns>
    /// <response code="200">Workflow found and returned</response>
    /// <response code="404">Workflow not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkflow(
        Guid id,
        [FromQuery] bool includeDefinition = true,
        [FromQuery] bool includeHistory = false,
        [FromQuery] bool includeMetrics = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetWorkflowQuery
            {
                Id = id,
                IncludeDefinition = includeDefinition,
                IncludeExecutionHistory = includeHistory,
                IncludeMetrics = includeMetrics
            };

            var result = await Mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(new ProblemDetails 
                { 
                    Title = "Workflow Not Found", 
                    Detail = result.ErrorMessage 
                });
            }

            var response = new GetWorkflowResponseDto
            {
                WorkflowId = result.WorkflowId,
                Name = result.Name,
                Description = result.Description,
                Category = result.Category,
                Status = result.Status,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                CreatedBy = result.CreatedBy,
                LastModifiedBy = result.LastModifiedBy,
                Version = result.Version,
                IsTemplate = result.IsTemplate,
                GlobalVariables = result.GlobalVariables,
                Metadata = result.Metadata,
                Definition = result.Definition,
                ExecutionHistory = result.ExecutionHistory,
                Metrics = result.Metrics
            };

            _logger.LogDebug("Retrieved workflow {WorkflowId}", id);

            return Ok(CreateSuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow: {WorkflowId}", id);
            return CreateErrorResponse("An error occurred while retrieving the workflow", ex.Message);
        }
    }

    /// <summary>
    /// Lists workflows with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="pageSize">Number of items per page (1-100)</param>
    /// <param name="pageNumber">Page number (starting from 1)</param>
    /// <param name="search">Search term for workflow name or description</param>
    /// <param name="category">Filter by category</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="orderBy">Sort field (Name, CreatedAt, UpdatedAt)</param>
    /// <param name="sortDesc">Sort in descending order</param>
    /// <param name="orderDesc">Sort in descending order</param>
    /// <param name="includeDefinition">Include workflow definitions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of workflows</returns>
    /// <response code="200">Workflows retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListWorkflowsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListWorkflows(
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string orderBy = "UpdatedAt",
        [FromQuery] bool sortDesc = true,
        [FromQuery] bool orderDesc = true,
        [FromQuery] bool includeDefinition = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ListWorkflowsQuery
            {
                PageSize = pageSize,
                Page = pageNumber,
                SearchTerm = search,
                Category = category,
                IsActive = isActive,
                SortBy = orderBy,
                SortDescending = sortDesc,
                OrderDescending = orderDesc,
                IncludeDefinition = includeDefinition
            };

            var result = await Mediator.Send(query, cancellationToken);

            // PaginatedResult doesn't have IsSuccess, so we check for valid data
            if (result == null || result.Items == null)
            {
                return BadRequest(new ProblemDetails 
                { 
                    Title = "List Workflows Failed", 
                    Detail = "Failed to retrieve workflows" 
                });
            }

            var response = new ListWorkflowsResponseDto
            {
                Workflows = result.Items.Select(w => new WorkflowSummaryDto
                {
                    WorkflowId = w.Id,
                    Name = w.Name,
                    Description = w.Description ?? string.Empty,
                    Category = w.Category ?? "General",
                    CreatedAt = w.CreatedAt
                }).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                PageNumber = result.Page,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };

            _logger.LogDebug("Retrieved {Count} workflows (page {Page}/{TotalPages})", 
                result.Items.Count, result.Page, result.TotalPages);

            return Ok(CreateSuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workflows");
            return CreateErrorResponse("An error occurred while listing workflows", ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing workflow definition.
    /// </summary>
    /// <param name="id">Workflow unique identifier</param>
    /// <param name="request">Workflow update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated workflow information</returns>
    /// <response code="200">Workflow updated successfully</response>
    /// <response code="400">Invalid workflow definition or validation errors</response>
    /// <response code="404">Workflow not found</response>
    /// <response code="409">Workflow is currently being executed</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateWorkflow(
        Guid id, 
        [FromBody] UpdateWorkflowRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateWorkflowCommand
            {
                WorkflowId = id,
                Name = request.Name,
                Description = request.Description,
                Category = request.Category ?? "General",
                Definition = request.Definition,
                GlobalVariables = request.GlobalVariables ?? new Dictionary<string, object>(),
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
                LastModifiedBy = User.Identity?.Name ?? "System",
                IncrementVersion = request.IncrementVersion ?? true
            };

            var result = await Mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    var problemDetails = new ValidationProblemDetails();
                    foreach (var error in result.ValidationErrors)
                    {
                        problemDetails.Errors.Add("ValidationError", new[] { error });
                    }
                    return BadRequest(problemDetails);
                }

                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new ProblemDetails 
                    { 
                        Title = "Workflow Not Found", 
                        Detail = result.ErrorMessage 
                    });
                }

                return BadRequest(new ProblemDetails 
                { 
                    Title = "Workflow Update Failed", 
                    Detail = result.ErrorMessage 
                });
            }

            var response = new UpdateWorkflowResponseDto
            {
                WorkflowId = result.WorkflowId,
                Name = result.Name,
                Version = result.NewVersion,
                UpdatedAt = result.UpdatedAt
            };

            _logger.LogInformation("Workflow {WorkflowId} '{Name}' updated to version {Version}", 
                result.WorkflowId, result.Name, result.NewVersion);

            return Ok(CreateSuccessResponse(response, "Workflow updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow: {WorkflowId}", id);
            return CreateErrorResponse("An error occurred while updating the workflow", ex.Message);
        }
    }

    /// <summary>
    /// Deletes a workflow and optionally its execution history.
    /// </summary>
    /// <param name="id">Workflow unique identifier</param>
    /// <param name="force">Force deletion even if workflow has active executions</param>
    /// <param name="deleteHistory">Also delete execution history</param>
    /// <param name="reason">Reason for deletion (for audit purposes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Workflow deleted successfully</response>
    /// <response code="404">Workflow not found</response>
    /// <response code="409">Workflow has active executions and force=false</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteWorkflow(
        Guid id,
        [FromQuery] bool force = false,
        [FromQuery] bool deleteHistory = false,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteWorkflowCommand
            {
                WorkflowId = id,
                ForceDelete = force,
                DeleteExecutionHistory = deleteHistory,
                Reason = reason,
                DeletedBy = User.Identity?.Name ?? "System"
            };

            var result = await Mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new ProblemDetails 
                    { 
                        Title = "Workflow Not Found", 
                        Detail = result.ErrorMessage 
                    });
                }

                if (result.ErrorMessage?.Contains("Cannot delete") == true)
                {
                    return Conflict(new ProblemDetails 
                    { 
                        Title = "Workflow Cannot Be Deleted", 
                        Detail = result.ErrorMessage 
                    });
                }

                return BadRequest(new ProblemDetails 
                { 
                    Title = "Workflow Deletion Failed", 
                    Detail = result.ErrorMessage 
                });
            }

            var response = new DeleteWorkflowResponseDto
            {
                WorkflowId = result.WorkflowId,
                Name = result.Name,
                DeletedAt = result.DeletedAt,
                RelatedRecordsDeleted = result.RelatedRecordsDeleted
            };

            _logger.LogInformation("Workflow {WorkflowId} '{Name}' deleted successfully", 
                result.WorkflowId, result.Name);

            return Ok(CreateSuccessResponse(response, "Workflow deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow: {WorkflowId}", id);
            return CreateErrorResponse("An error occurred while deleting the workflow", ex.Message);
        }
    }

    /// <summary>
    /// Validates a workflow definition without saving it.
    /// </summary>
    /// <param name="request">Workflow validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation results with errors, warnings, and suggestions</returns>
    /// <response code="200">Validation completed (check IsValid property)</response>
    /// <response code="400">Invalid request format</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateWorkflow(
        [FromBody] ValidateWorkflowRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ValidateWorkflowCommand
            {
                Definition = request.Definition,
                StrictValidation = request.StrictValidation ?? false,
                CheckNodeCompatibility = request.CheckNodeCompatibility ?? true,
                ValidateConnections = request.ValidateConnections ?? true,
                CheckCircularDependencies = request.CheckCircularDependencies ?? true,
                TestInputData = request.TestInputData
            };

            var result = await Mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails 
                { 
                    Title = "Workflow Validation Failed", 
                    Detail = result.ErrorMessage 
                });
            }

            var response = new ValidateWorkflowResponseDto
            {
                IsValid = result.IsValid,
                Errors = result.Errors,
                Warnings = result.Warnings,
                Suggestions = result.Suggestions,
                Summary = result.Summary
            };

            _logger.LogDebug("Workflow validation completed: {IsValid} (Errors: {ErrorCount}, Warnings: {WarningCount})",
                result.IsValid, result.Errors.Count, result.Warnings.Count);

            return Ok(CreateSuccessResponse(response, 
                result.IsValid ? "Workflow is valid" : $"Workflow validation failed with {result.Errors.Count} errors"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workflow");
            return CreateErrorResponse("An error occurred while validating the workflow", ex.Message);
        }
    }
}

// DTOs for API requests and responses
public record CreateWorkflowRequestDto
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; init; } = string.Empty;
    
    [StringLength(100)]
    public string? Category { get; init; }
    
    public string? Priority { get; init; }
    
    [Range(1, 10080)] // 1 minute to 1 week
    public int? TimeoutMinutes { get; init; }
    
    [Range(1, 1000)]
    public int? MaxConcurrentExecutions { get; init; }
    
    public bool? IsTemplate { get; init; }
    
    public Dictionary<string, object>? GlobalVariables { get; init; }
}

public record CreateWorkflowResponseDto
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int Version { get; init; }
}

public record GetWorkflowResponseDto
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
    public WorkflowDefinitionDto? Definition { get; init; }
    public List<ExecutionHistoryDto>? ExecutionHistory { get; init; }
    public WorkflowMetricsDto? Metrics { get; init; }
}

public record ListWorkflowsResponseDto
{
    public List<WorkflowSummaryDto> Workflows { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}

public record UpdateWorkflowRequestDto
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; init; } = string.Empty;
    
    [StringLength(100)]
    public string? Category { get; init; }
    
    public WorkflowDefinitionDto? Definition { get; init; }
    public Dictionary<string, object>? GlobalVariables { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
    public bool? IncrementVersion { get; init; }
}

public record UpdateWorkflowResponseDto
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record DeleteWorkflowResponseDto
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
    public int RelatedRecordsDeleted { get; init; }
}

public record ValidateWorkflowRequestDto
{
    [Required]
    public WorkflowDefinitionDto Definition { get; init; } = new();
    
    public bool? StrictValidation { get; init; }
    public bool? CheckNodeCompatibility { get; init; }
    public bool? ValidateConnections { get; init; }
    public bool? CheckCircularDependencies { get; init; }
    public Dictionary<string, object>? TestInputData { get; init; }
}

public record ValidateWorkflowResponseDto
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
    public List<ValidationWarning> Warnings { get; init; } = new();
    public List<ValidationSuggestion> Suggestions { get; init; } = new();
    public ValidationSummary Summary { get; init; } = new();
}
