using Microsoft.AspNetCore.SignalR;

namespace WorkflowPlatform.API.Hubs;

/// <summary>
/// Service for broadcasting workflow execution updates via SignalR.
/// Provides methods to send real-time updates to subscribed clients.
/// </summary>
public class WorkflowExecutionBroadcastService
{
    private readonly IHubContext<WorkflowExecutionHub> _hubContext;
    private readonly ILogger<WorkflowExecutionBroadcastService> _logger;

    public WorkflowExecutionBroadcastService(
        IHubContext<WorkflowExecutionHub> hubContext,
        ILogger<WorkflowExecutionBroadcastService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Broadcasts workflow execution started event to all subscribers.
    /// </summary>
    public async Task BroadcastExecutionStarted(Guid workflowId, Guid executionId, string workflowName, 
        DateTime startedAt, string executedBy, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var update = new
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Status = "Started",
                StartedAt = startedAt,
                ExecutedBy = executedBy,
                ProgressPercentage = 0,
                CurrentNode = "initialization",
                Metadata = metadata ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers
            await _hubContext.Clients.Group($"Workflow_{workflowId}").SendAsync("ExecutionStarted", update);
            
            // Send to execution-specific subscribers  
            await _hubContext.Clients.Group($"Execution_{executionId}").SendAsync("ExecutionStarted", update);

            // Send to all connected users (for global monitoring)
            await _hubContext.Clients.Group("AllUsers").SendAsync("GlobalExecutionStarted", new
            {
                update.ExecutionId,
                update.WorkflowId,
                update.WorkflowName,
                update.StartedAt,
                update.ExecutedBy
            });

            _logger.LogInformation("Broadcasted execution started: {ExecutionId} for workflow {WorkflowId}", 
                executionId, workflowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting execution started for {ExecutionId}", executionId);
        }
    }

    /// <summary>
    /// Broadcasts workflow execution progress update to subscribers.
    /// </summary>
    public async Task BroadcastExecutionProgress(Guid executionId, string currentNodeId, string currentNodeName, 
        int progressPercentage, string? outputData = null, Dictionary<string, object>? nodeMetrics = null)
    {
        try
        {
            var update = new
            {
                ExecutionId = executionId,
                Status = "Running",
                CurrentNodeId = currentNodeId,
                CurrentNodeName = currentNodeName,
                ProgressPercentage = progressPercentage,
                OutputData = outputData,
                NodeMetrics = nodeMetrics ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to execution-specific subscribers
            await _hubContext.Clients.Group($"Execution_{executionId}").SendAsync("ExecutionProgress", update);

            _logger.LogDebug("Broadcasted execution progress: {ExecutionId} at {Progress}% (Node: {NodeName})", 
                executionId, progressPercentage, currentNodeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting execution progress for {ExecutionId}", executionId);
        }
    }

    /// <summary>
    /// Broadcasts workflow execution completed event to subscribers.
    /// </summary>
    public async Task BroadcastExecutionCompleted(Guid workflowId, Guid executionId, string workflowName,
        DateTime completedAt, TimeSpan duration, string? outputData = null, Dictionary<string, object>? finalMetrics = null)
    {
        try
        {
            var update = new
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Status = "Completed",
                CompletedAt = completedAt,
                Duration = duration,
                ProgressPercentage = 100,
                OutputData = outputData,
                FinalMetrics = finalMetrics ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers
            await _hubContext.Clients.Group($"Workflow_{workflowId}").SendAsync("ExecutionCompleted", update);
            
            // Send to execution-specific subscribers
            await _hubContext.Clients.Group($"Execution_{executionId}").SendAsync("ExecutionCompleted", update);

            // Send to all connected users
            await _hubContext.Clients.Group("AllUsers").SendAsync("GlobalExecutionCompleted", new
            {
                update.ExecutionId,
                update.WorkflowId,
                update.WorkflowName,
                update.CompletedAt,
                update.Duration
            });

            _logger.LogInformation("Broadcasted execution completed: {ExecutionId} for workflow {WorkflowId} in {Duration}", 
                executionId, workflowId, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting execution completed for {ExecutionId}", executionId);
        }
    }

    /// <summary>
    /// Broadcasts workflow execution failed event to subscribers.
    /// </summary>
    public async Task BroadcastExecutionFailed(Guid workflowId, Guid executionId, string workflowName,
        DateTime failedAt, string errorMessage, string? failedNodeId = null, string? failedNodeName = null,
        Dictionary<string, object>? errorMetrics = null)
    {
        try
        {
            var update = new
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Status = "Failed",
                FailedAt = failedAt,
                ErrorMessage = errorMessage,
                FailedNodeId = failedNodeId,
                FailedNodeName = failedNodeName,
                ErrorMetrics = errorMetrics ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers
            await _hubContext.Clients.Group($"Workflow_{workflowId}").SendAsync("ExecutionFailed", update);
            
            // Send to execution-specific subscribers
            await _hubContext.Clients.Group($"Execution_{executionId}").SendAsync("ExecutionFailed", update);

            // Send to all connected users
            await _hubContext.Clients.Group("AllUsers").SendAsync("GlobalExecutionFailed", new
            {
                update.ExecutionId,
                update.WorkflowId,
                update.WorkflowName,
                update.FailedAt,
                ErrorSummary = errorMessage.Length > 100 ? errorMessage.Substring(0, 100) + "..." : errorMessage
            });

            _logger.LogWarning("Broadcasted execution failed: {ExecutionId} for workflow {WorkflowId}. Error: {ErrorMessage}", 
                executionId, workflowId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting execution failed for {ExecutionId}", executionId);
        }
    }

    /// <summary>
    /// Broadcasts workflow execution cancelled event to subscribers.
    /// </summary>
    public async Task BroadcastExecutionCancelled(Guid workflowId, Guid executionId, string workflowName,
        DateTime cancelledAt, string cancelledBy, string? reason = null)
    {
        try
        {
            var update = new
            {
                ExecutionId = executionId,
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Status = "Cancelled",
                CancelledAt = cancelledAt,
                CancelledBy = cancelledBy,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers
            await _hubContext.Clients.Group($"Workflow_{workflowId}").SendAsync("ExecutionCancelled", update);
            
            // Send to execution-specific subscribers
            await _hubContext.Clients.Group($"Execution_{executionId}").SendAsync("ExecutionCancelled", update);

            // Send to all connected users
            await _hubContext.Clients.Group("AllUsers").SendAsync("GlobalExecutionCancelled", new
            {
                update.ExecutionId,
                update.WorkflowId,
                update.WorkflowName,
                update.CancelledAt,
                update.CancelledBy
            });

            _logger.LogInformation("Broadcasted execution cancelled: {ExecutionId} for workflow {WorkflowId} by {CancelledBy}", 
                executionId, workflowId, cancelledBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting execution cancelled for {ExecutionId}", executionId);
        }
    }

    /// <summary>
    /// Broadcasts workflow definition updated event to subscribers.
    /// </summary>
    public async Task BroadcastWorkflowUpdated(Guid workflowId, string workflowName, int newVersion, 
        DateTime updatedAt, string updatedBy, List<string>? changedProperties = null)
    {
        try
        {
            var update = new
            {
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Version = newVersion,
                UpdatedAt = updatedAt,
                UpdatedBy = updatedBy,
                ChangedProperties = changedProperties ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers
            await _hubContext.Clients.Group($"Workflow_{workflowId}").SendAsync("WorkflowUpdated", update);

            // Send to all connected users (for global monitoring)
            await _hubContext.Clients.Group("AllUsers").SendAsync("GlobalWorkflowUpdated", new
            {
                update.WorkflowId,
                update.WorkflowName,
                update.Version,
                update.UpdatedAt,
                update.UpdatedBy
            });

            _logger.LogInformation("Broadcasted workflow updated: {WorkflowId} version {Version} by {UpdatedBy}", 
                workflowId, newVersion, updatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting workflow updated for {WorkflowId}", workflowId);
        }
    }

    /// <summary>
    /// Broadcasts system-wide notification to all connected users.
    /// </summary>
    public async Task BroadcastSystemNotification(string title, string message, string severity = "info", 
        Dictionary<string, object>? metadata = null)
    {
        try
        {
            var notification = new
            {
                Title = title,
                Message = message,
                Severity = severity, // info, warning, error, success
                Metadata = metadata ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("AllUsers").SendAsync("SystemNotification", notification);

            _logger.LogInformation("Broadcasted system notification: {Title} ({Severity})", title, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system notification: {Title}", title);
        }
    }

    /// <summary>
    /// Broadcasts real-time metrics update to subscribers.
    /// </summary>
    public async Task BroadcastMetricsUpdate(Dictionary<string, object> metrics, string? scope = null)
    {
        try
        {
            var update = new
            {
                Metrics = metrics,
                Scope = scope ?? "global",
                Timestamp = DateTime.UtcNow
            };

            var groupName = scope != null ? $"Metrics_{scope}" : "AllUsers";
            await _hubContext.Clients.Group(groupName).SendAsync("MetricsUpdate", update);

            _logger.LogDebug("Broadcasted metrics update for scope: {Scope}", scope ?? "global");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting metrics update for scope: {Scope}", scope);
        }
    }

    /// <summary>
    /// Broadcasts collaboration event (for workflow editing).
    /// </summary>
    public async Task BroadcastCollaborationEvent(Guid workflowId, string eventType, string userId, string userName,
        Dictionary<string, object>? eventData = null)
    {
        try
        {
            var collaborationEvent = new
            {
                WorkflowId = workflowId,
                EventType = eventType, // node_selected, node_moved, connection_created, etc.
                UserId = userId,
                UserName = userName,
                EventData = eventData ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            // Send to workflow-specific subscribers (broadcast to all subscribers)
            await _hubContext.Clients.Group($"Workflow_{workflowId}")
                .SendAsync("CollaborationEvent", collaborationEvent);

            _logger.LogDebug("Broadcasted collaboration event: {EventType} for workflow {WorkflowId} by {UserName}", 
                eventType, workflowId, userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting collaboration event for workflow {WorkflowId}", workflowId);
        }
    }

    /// <summary>
    /// Gets the count of active subscribers for a workflow.
    /// </summary>
    public async Task<int> GetWorkflowSubscriberCount(Guid workflowId)
    {
        try
        {
            // This is a simplified implementation. In a real scenario, you might want to use Redis
            // or another distributed cache to track subscribers across multiple server instances.
            
            // For now, we'll use SignalR's built-in groups functionality
            // Note: SignalR doesn't provide a direct way to count group members, 
            // so this would need custom implementation in production

            return await Task.FromResult(0); // Placeholder - implement based on your tracking needs
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriber count for workflow {WorkflowId}", workflowId);
            return await Task.FromResult(0);
        }
    }
}
