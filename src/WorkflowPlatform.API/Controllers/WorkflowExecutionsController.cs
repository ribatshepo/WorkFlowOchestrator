using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using WorkflowPlatform.Application.Workflows.Commands.ExecuteWorkflow;

namespace WorkflowPlatform.API.Controllers;

/// <summary>
/// Controller for workflow execution operations.
/// Provides REST API endpoints for executing workflows and monitoring execution status.
/// </summary>
[Authorize]
[EnableRateLimiting("ExecutionPolicy")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflows/{workflowId:guid}/executions")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
public class WorkflowExecutionsController : ApiControllerBase
{
    private readonly ILogger<WorkflowExecutionsController> _logger;

    public WorkflowExecutionsController(ILogger<WorkflowExecutionsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a workflow with the provided input data and context.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    /// <param name="request">Execution request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution status and tracking information</returns>
    /// <response code="202">Execution started successfully</response>
    /// <response code="400">Invalid execution parameters</response>
    /// <response code="404">Workflow not found</response>
    /// <response code="409">Workflow cannot be executed (inactive, reached max concurrent executions)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExecuteWorkflowResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExecuteWorkflow(
        Guid workflowId,
        [FromBody] ExecuteWorkflowRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ExecuteWorkflowCommand
            {
                WorkflowId = workflowId,
                InputData = request.InputData ?? "{}",
                ExecutionContext = request.ExecutionContext ?? new Dictionary<string, string>(),
                Timeout = request.TimeoutMinutes.HasValue ? TimeSpan.FromMinutes(request.TimeoutMinutes.Value) : null,
                ExecutedBy = User.Identity?.Name ?? "System",
                Priority = request.Priority ?? 0,
                EnableRealTimeUpdates = request.EnableRealTimeUpdates ?? true,
                NotificationChannels = request.NotificationChannels
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

                if (result.ErrorMessage?.Contains("not in an active state") == true ||
                    result.ErrorMessage?.Contains("maximum concurrent") == true)
                {
                    return Conflict(new ProblemDetails
                    {
                        Title = "Workflow Cannot Be Executed",
                        Detail = result.ErrorMessage
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Workflow Execution Failed",
                    Detail = result.ErrorMessage
                });
            }

            var response = new ExecuteWorkflowResponseDto
            {
                ExecutionId = result.ExecutionId,
                WorkflowId = result.WorkflowId,
                WorkflowName = result.WorkflowName,
                Status = result.Status,
                StartedAt = result.StartedAt,
                EstimatedDuration = result.EstimatedDuration,
                ExecutionMetadata = result.ExecutionMetadata,
                StatusUrl = Url.Action("GetExecutionStatus", "WorkflowExecutions", new { workflowId, executionId = result.ExecutionId }),
                CancelUrl = Url.Action("CancelExecution", "WorkflowExecutions", new { workflowId, executionId = result.ExecutionId })
            };

            _logger.LogInformation("Workflow execution {ExecutionId} started for workflow {WorkflowId} '{WorkflowName}'",
                result.ExecutionId, result.WorkflowId, result.WorkflowName);

            return Accepted(response.StatusUrl, CreateSuccessResponse(response, "Workflow execution started successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow: {WorkflowId}", workflowId);
            return CreateErrorResponse("An error occurred while executing the workflow", ex.Message);
        }
    }

    /// <summary>
    /// Gets the status and details of a workflow execution.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    /// <param name="executionId">Execution unique identifier</param>
    /// <param name="includeLogs">Include execution logs in response</param>
    /// <param name="includeNodeDetails">Include individual node execution details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution status and details</returns>
    /// <response code="200">Execution status retrieved successfully</response>
    /// <response code="404">Workflow or execution not found</response>
    [HttpGet("{executionId:guid}")]
    [ProducesResponseType(typeof(ExecutionStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionStatus(
        Guid workflowId,
        Guid executionId,
        [FromQuery] bool includeLogs = false,
        [FromQuery] bool includeNodeDetails = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // This would be implemented with a query handler
            // For now, returning a mock response structure
            _logger.LogDebug("Getting execution status for {ExecutionId} of workflow {WorkflowId}", executionId, workflowId);

            var response = new ExecutionStatusResponseDto
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                WorkflowName = "Sample Workflow", // Would come from the query result
                Status = "Running",
                StartedAt = DateTime.UtcNow.AddMinutes(-5),
                CompletedAt = null,
                Duration = TimeSpan.FromMinutes(5),
                ProgressPercentage = 45,
                CurrentNodeId = "node_2",
                CurrentNodeName = "Data Processing",
                OutputData = null,
                ErrorMessage = null,
                ExecutedBy = User.Identity?.Name ?? "System",
                NodeResults = includeNodeDetails ? new List<NodeExecutionResultDto>
                {
                    new() { NodeId = "node_1", NodeName = "Start", Status = "Completed", StartedAt = DateTime.UtcNow.AddMinutes(-5), CompletedAt = DateTime.UtcNow.AddMinutes(-4) },
                    new() { NodeId = "node_2", NodeName = "Data Processing", Status = "Running", StartedAt = DateTime.UtcNow.AddMinutes(-2), CompletedAt = null }
                } : new List<NodeExecutionResultDto>(),
                ExecutionLogs = includeLogs ? new List<ExecutionLogDto>
                {
                    new() { Timestamp = DateTime.UtcNow.AddMinutes(-5), Level = "Info", Message = "Execution started", NodeId = null },
                    new() { Timestamp = DateTime.UtcNow.AddMinutes(-4), Level = "Info", Message = "Node completed successfully", NodeId = "node_1" },
                    new() { Timestamp = DateTime.UtcNow.AddMinutes(-2), Level = "Info", Message = "Processing data...", NodeId = "node_2" }
                } : new List<ExecutionLogDto>(),
                Metrics = new ExecutionMetricsDto
                {
                    ExecutionTimeMs = (long)TimeSpan.FromMinutes(5).TotalMilliseconds,
                    NodesExecuted = 1,
                    NodesCompleted = 1,
                    NodesFailed = 0,
                    MemoryUsageBytes = 1024 * 1024 * 50, // 50MB
                    CpuUsagePercentage = 25
                }
            };

            return await Task.FromResult(Ok(CreateSuccessResponse(response)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution status: {ExecutionId}", executionId);
            return CreateErrorResponse("An error occurred while retrieving execution status", ex.Message);
        }
    }

    /// <summary>
    /// Lists all executions for a workflow with filtering and pagination.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    /// <param name="pageSize">Number of items per page (1-100)</param>
    /// <param name="pageNumber">Page number (starting from 1)</param>
    /// <param name="status">Filter by execution status</param>
    /// <param name="executedBy">Filter by executor</param>
    /// <param name="startedAfter">Filter executions started after this date</param>
    /// <param name="startedBefore">Filter executions started before this date</param>
    /// <param name="orderBy">Sort field (StartedAt, CompletedAt, Duration)</param>
    /// <param name="orderDesc">Sort in descending order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of workflow executions</returns>
    /// <response code="200">Executions retrieved successfully</response>
    /// <response code="404">Workflow not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListExecutionsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListExecutions(
        Guid workflowId,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery] string? status = null,
        [FromQuery] string? executedBy = null,
        [FromQuery] DateTime? startedAfter = null,
        [FromQuery] DateTime? startedBefore = null,
        [FromQuery] string orderBy = "StartedAt",
        [FromQuery] bool orderDesc = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // This would be implemented with a query handler
            _logger.LogDebug("Listing executions for workflow {WorkflowId}", workflowId);

            var response = new ListExecutionsResponseDto
            {
                Executions = new List<ExecutionSummaryDto>
                {
                    new()
                    {
                        ExecutionId = Guid.NewGuid(),
                        WorkflowId = workflowId,
                        Status = "Completed",
                        StartedAt = DateTime.UtcNow.AddHours(-2),
                        CompletedAt = DateTime.UtcNow.AddHours(-1),
                        Duration = TimeSpan.FromHours(1),
                        ExecutedBy = "user@example.com",
                        NodesExecuted = 5,
                        NodesCompleted = 5,
                        NodesFailed = 0
                    }
                },
                TotalCount = 1,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };

            return await Task.FromResult(Ok(CreateSuccessResponse(response)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing executions for workflow: {WorkflowId}", workflowId);
            return await Task.FromResult(CreateErrorResponse("An error occurred while listing executions", ex.Message));
        }
    }

    /// <summary>
    /// Cancels a running workflow execution.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    /// <param name="executionId">Execution unique identifier</param>
    /// <param name="request">Cancellation request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation confirmation</returns>
    /// <response code="200">Execution cancelled successfully</response>
    /// <response code="404">Workflow or execution not found</response>
    /// <response code="409">Execution cannot be cancelled (already completed or failed)</response>
    [HttpPost("{executionId:guid}/cancel")]
    [ProducesResponseType(typeof(CancelExecutionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelExecution(
        Guid workflowId,
        Guid executionId,
        [FromBody] CancelExecutionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // This would be implemented with a command handler
            _logger.LogInformation("Cancelling execution {ExecutionId} for workflow {WorkflowId}. Reason: {Reason}",
                executionId, workflowId, request.Reason);

            var response = new CancelExecutionResponseDto
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                Status = "Cancelled",
                CancelledAt = DateTime.UtcNow,
                CancelledBy = User.Identity?.Name ?? "System",
                Reason = request.Reason
            };

            return await Task.FromResult(Ok(CreateSuccessResponse(response, "Execution cancelled successfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution: {ExecutionId}", executionId);
            return CreateErrorResponse("An error occurred while cancelling the execution", ex.Message);
        }
    }
}

// DTOs for execution API requests and responses
public record ExecuteWorkflowRequestDto
{
    [Required]
    public string InputData { get; init; } = "{}";
    
    public Dictionary<string, string>? ExecutionContext { get; init; }
    
    [Range(1, 10080)] // 1 minute to 1 week
    public int? TimeoutMinutes { get; init; }
    
    [Range(-100, 100)]
    public int? Priority { get; init; }
    
    public bool? EnableRealTimeUpdates { get; init; }
    
    public List<string>? NotificationChannels { get; init; }
}

public record ExecuteWorkflowResponseDto
{
    public Guid ExecutionId { get; init; }
    public Guid WorkflowId { get; init; }
    public string WorkflowName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public TimeSpan? EstimatedDuration { get; init; }
    public Dictionary<string, object> ExecutionMetadata { get; init; } = new();
    public string? StatusUrl { get; init; }
    public string? CancelUrl { get; init; }
}

public record ExecutionStatusResponseDto
{
    public Guid ExecutionId { get; init; }
    public Guid WorkflowId { get; init; }
    public string WorkflowName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan Duration { get; init; }
    public int ProgressPercentage { get; init; }
    public string? CurrentNodeId { get; init; }
    public string? CurrentNodeName { get; init; }
    public string? OutputData { get; init; }
    public string? ErrorMessage { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public List<NodeExecutionResultDto> NodeResults { get; init; } = new();
    public List<ExecutionLogDto> ExecutionLogs { get; init; } = new();
    public ExecutionMetricsDto Metrics { get; init; } = new();
}

public record NodeExecutionResultDto
{
    public string NodeId { get; init; } = string.Empty;
    public string NodeName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? OutputData { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ExecutionLogDto
{
    public DateTime Timestamp { get; init; }
    public string Level { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? NodeId { get; init; }
    public Dictionary<string, object> Properties { get; init; } = new();
}

public record ExecutionMetricsDto
{
    public long ExecutionTimeMs { get; init; }
    public int NodesExecuted { get; init; }
    public int NodesCompleted { get; init; }
    public int NodesFailed { get; init; }
    public long MemoryUsageBytes { get; init; }
    public int CpuUsagePercentage { get; init; }
    public Dictionary<string, object> CustomMetrics { get; init; } = new();
}

public record ListExecutionsResponseDto
{
    public List<ExecutionSummaryDto> Executions { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}

public record ExecutionSummaryDto
{
    public Guid ExecutionId { get; init; }
    public Guid WorkflowId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public int NodesExecuted { get; init; }
    public int NodesCompleted { get; init; }
    public int NodesFailed { get; init; }
    public string? ErrorMessage { get; init; }
}

public record CancelExecutionRequestDto
{
    [StringLength(500)]
    public string? Reason { get; init; }
}

public record CancelExecutionResponseDto
{
    public Guid ExecutionId { get; init; }
    public Guid WorkflowId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CancelledAt { get; init; }
    public string CancelledBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
