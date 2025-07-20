using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp;
using System.Security.Claims;

namespace WorkflowPlatform.API.Hubs;

/// <summary>
/// SignalR hub for real-time workflow execution updates and monitoring.
/// Provides live status broadcasting, progress updates, and execution logs.
/// </summary>
[Authorize]
public class WorkflowExecutionHub : Hub
{
    private readonly ILogger<WorkflowExecutionHub> _logger;
    private static readonly Dictionary<string, HashSet<string>> _workflowSubscriptions = new();
    private static readonly Dictionary<string, HashSet<string>> _executionSubscriptions = new();
    private static readonly object _lock = new object();

    public WorkflowExecutionHub(ILogger<WorkflowExecutionHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        _logger.LogInformation("SignalR client connected: {ConnectionId} for user {UserName} ({UserId})", 
            Context.ConnectionId, userName, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");
        
        // Send welcome message with connection info
        await Clients.Caller.SendAsync("Connected", new
        {
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            UserName = userName,
            ConnectedAt = DateTime.UtcNow,
            Message = "Successfully connected to Workflow Execution Hub"
        });

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        _logger.LogInformation("SignalR client disconnected: {ConnectionId} for user {UserName} ({UserId}). Exception: {Exception}", 
            Context.ConnectionId, userName, userId, exception?.Message);

        // Clean up subscriptions
        await UnsubscribeFromAllWorkflows();
        await UnsubscribeFromAllExecutions();

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to real-time updates for a specific workflow.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    public async Task SubscribeToWorkflow(string workflowId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(workflowId) || !Guid.TryParse(workflowId, out _))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid workflow ID format" });
                return;
            }

            var groupName = $"Workflow_{workflowId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            lock (_lock)
            {
                if (!_workflowSubscriptions.ContainsKey(workflowId))
                {
                    _workflowSubscriptions[workflowId] = new HashSet<string>();
                }
                _workflowSubscriptions[workflowId].Add(Context.ConnectionId);
            }

            _logger.LogDebug("Client {ConnectionId} subscribed to workflow {WorkflowId}", 
                Context.ConnectionId, workflowId);

            await Clients.Caller.SendAsync("WorkflowSubscribed", new
            {
                WorkflowId = workflowId,
                SubscribedAt = DateTime.UtcNow,
                Message = $"Successfully subscribed to workflow {workflowId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to workflow {WorkflowId} for connection {ConnectionId}", 
                workflowId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to subscribe to workflow" });
        }
    }

    /// <summary>
    /// Unsubscribe from real-time updates for a specific workflow.
    /// </summary>
    /// <param name="workflowId">Workflow unique identifier</param>
    public async Task UnsubscribeFromWorkflow(string workflowId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(workflowId) || !Guid.TryParse(workflowId, out _))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid workflow ID format" });
                return;
            }

            var groupName = $"Workflow_{workflowId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            lock (_lock)
            {
                if (_workflowSubscriptions.ContainsKey(workflowId))
                {
                    _workflowSubscriptions[workflowId].Remove(Context.ConnectionId);
                    if (_workflowSubscriptions[workflowId].Count == 0)
                    {
                        _workflowSubscriptions.Remove(workflowId);
                    }
                }
            }

            _logger.LogDebug("Client {ConnectionId} unsubscribed from workflow {WorkflowId}", 
                Context.ConnectionId, workflowId);

            await Clients.Caller.SendAsync("WorkflowUnsubscribed", new
            {
                WorkflowId = workflowId,
                UnsubscribedAt = DateTime.UtcNow,
                Message = $"Successfully unsubscribed from workflow {workflowId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from workflow {WorkflowId} for connection {ConnectionId}", 
                workflowId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to unsubscribe from workflow" });
        }
    }

    /// <summary>
    /// Subscribe to real-time updates for a specific execution.
    /// </summary>
    /// <param name="executionId">Execution unique identifier</param>
    public async Task SubscribeToExecution(string executionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid execution ID format" });
                return;
            }

            var groupName = $"Execution_{executionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            lock (_lock)
            {
                if (!_executionSubscriptions.ContainsKey(executionId))
                {
                    _executionSubscriptions[executionId] = new HashSet<string>();
                }
                _executionSubscriptions[executionId].Add(Context.ConnectionId);
            }

            _logger.LogDebug("Client {ConnectionId} subscribed to execution {ExecutionId}", 
                Context.ConnectionId, executionId);

            await Clients.Caller.SendAsync("ExecutionSubscribed", new
            {
                ExecutionId = executionId,
                SubscribedAt = DateTime.UtcNow,
                Message = $"Successfully subscribed to execution {executionId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to execution {ExecutionId} for connection {ConnectionId}", 
                executionId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to subscribe to execution" });
        }
    }

    /// <summary>
    /// Unsubscribe from real-time updates for a specific execution.
    /// </summary>
    /// <param name="executionId">Execution unique identifier</param>
    public async Task UnsubscribeFromExecution(string executionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid execution ID format" });
                return;
            }

            var groupName = $"Execution_{executionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            lock (_lock)
            {
                if (_executionSubscriptions.ContainsKey(executionId))
                {
                    _executionSubscriptions[executionId].Remove(Context.ConnectionId);
                    if (_executionSubscriptions[executionId].Count == 0)
                    {
                        _executionSubscriptions.Remove(executionId);
                    }
                }
            }

            _logger.LogDebug("Client {ConnectionId} unsubscribed from execution {ExecutionId}", 
                Context.ConnectionId, executionId);

            await Clients.Caller.SendAsync("ExecutionUnsubscribed", new
            {
                ExecutionId = executionId,
                UnsubscribedAt = DateTime.UtcNow,
                Message = $"Successfully unsubscribed from execution {executionId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from execution {ExecutionId} for connection {ConnectionId}", 
                executionId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to unsubscribe from execution" });
        }
    }

    /// <summary>
    /// Join a user group for collaborative features.
    /// </summary>
    /// <param name="groupName">Group name to join</param>
    public async Task JoinGroup(string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Group name cannot be empty" });
                return;
            }

            // Sanitize group name
            groupName = groupName.Replace(" ", "_").ToLowerInvariant();
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupJoined", new
            {
                GroupName = groupName,
                JoinedAt = DateTime.UtcNow,
                Message = $"Successfully joined group {groupName}"
            });

            // Notify other group members
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedGroup", new
            {
                GroupName = groupName,
                UserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = Context.User?.FindFirst(ClaimTypes.Name)?.Value,
                JoinedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupName} for connection {ConnectionId}", 
                groupName, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join group" });
        }
    }

    /// <summary>
    /// Leave a user group.
    /// </summary>
    /// <param name="groupName">Group name to leave</param>
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Group name cannot be empty" });
                return;
            }

            // Sanitize group name
            groupName = groupName.Replace(" ", "_").ToLowerInvariant();
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupLeft", new
            {
                GroupName = groupName,
                LeftAt = DateTime.UtcNow,
                Message = $"Successfully left group {groupName}"
            });

            // Notify other group members
            await Clients.OthersInGroup(groupName).SendAsync("UserLeftGroup", new
            {
                GroupName = groupName,
                UserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = Context.User?.FindFirst(ClaimTypes.Name)?.Value,
                LeftAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupName} for connection {ConnectionId}", 
                groupName, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to leave group" });
        }
    }

    /// <summary>
    /// Send a message to all users in a group (collaborative editing).
    /// </summary>
    /// <param name="groupName">Target group name</param>
    /// <param name="message">Message content</param>
    public async Task SendMessageToGroup(string groupName, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Group name and message cannot be empty" });
                return;
            }

            // Sanitize group name
            groupName = groupName.Replace(" ", "_").ToLowerInvariant();
            
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            await Clients.Group(groupName).SendAsync("GroupMessage", new
            {
                GroupName = groupName,
                Message = message,
                SenderId = userId,
                SenderName = userName,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogDebug("Message sent to group {GroupName} by {UserName} ({UserId})", 
                groupName, userName, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to group {GroupName} for connection {ConnectionId}", 
                groupName, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message to group" });
        }
    }

    /// <summary>
    /// Request current status of a workflow execution.
    /// </summary>
    /// <param name="executionId">Execution unique identifier</param>
    public async Task RequestExecutionStatus(string executionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid execution ID format" });
                return;
            }

            // This would typically query the actual execution status from the database
            // For now, returning a mock status
            await Clients.Caller.SendAsync("ExecutionStatus", new
            {
                ExecutionId = executionId,
                WorkflowId = Guid.NewGuid().ToString(),
                Status = "Running",
                ProgressPercentage = 65,
                CurrentNode = "node_processing",
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                EstimatedCompletion = DateTime.UtcNow.AddMinutes(5),
                NodesCompleted = 3,
                TotalNodes = 5,
                RequestedAt = DateTime.UtcNow
            });

            _logger.LogDebug("Execution status requested for {ExecutionId} by connection {ConnectionId}", 
                executionId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting execution status {ExecutionId} for connection {ConnectionId}", 
                executionId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to get execution status" });
        }
    }

    private async Task UnsubscribeFromAllWorkflows()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var workflowsToRemove = new List<string>();
                foreach (var (workflowId, connections) in _workflowSubscriptions.ToList())
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        workflowsToRemove.Add(workflowId);
                    }
                }

                foreach (var workflowId in workflowsToRemove)
                {
                    _workflowSubscriptions.Remove(workflowId);
                }
            }
        });
        _logger.LogDebug("Unsubscribed client {ConnectionId} from all workflows", Context.ConnectionId);
    }

    private async Task UnsubscribeFromAllExecutions()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var executionsToRemove = new List<string>();
                foreach (var (executionId, connections) in _executionSubscriptions.ToList())
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        executionsToRemove.Add(executionId);
                    }
                }

                foreach (var executionId in executionsToRemove)
                {
                    _executionSubscriptions.Remove(executionId);
                }
            }
        });
        _logger.LogDebug("Unsubscribed client {ConnectionId} from all executions", Context.ConnectionId);
    }
}
