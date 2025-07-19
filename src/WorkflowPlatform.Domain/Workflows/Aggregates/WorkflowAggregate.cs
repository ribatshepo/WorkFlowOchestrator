using WorkflowPlatform.Domain.Common.Primitives;
using WorkflowPlatform.Domain.Common.Enumerations;
using WorkflowPlatform.Domain.Common.Exceptions;
using WorkflowPlatform.Domain.Workflows.Entities;
using WorkflowPlatform.Domain.Workflows.Events;
using WorkflowPlatform.Domain.Workflows.ValueObjects;

namespace WorkflowPlatform.Domain.Workflows.Aggregates;

/// <summary>
/// Aggregate root representing a complete workflow definition.
/// Encapsulates all nodes, connections, and business rules for workflow execution.
/// </summary>
public class WorkflowAggregate : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public WorkflowPriority Priority { get; private set; }
    public string Category { get; private set; }
    public string? TriggerConfiguration { get; private set; }
    public TimeSpan? DefaultTimeout { get; private set; }
    public int MaxConcurrentExecutions { get; private set; }
    public bool IsTemplate { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public int TotalExecutions { get; private set; }
    public Dictionary<string, object> GlobalVariables { get; private set; }

    // Navigation properties
    public IReadOnlyList<WorkflowNode> Nodes => _nodes.AsReadOnly();
    public IReadOnlyList<WorkflowConnection> Connections => _connections.AsReadOnly();

    private readonly List<WorkflowNode> _nodes = new();
    private readonly List<WorkflowConnection> _connections = new();

    // Private constructor for EF Core
    private WorkflowAggregate() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        GlobalVariables = new Dictionary<string, object>();
    }

    private WorkflowAggregate(
        Guid id,
        string name,
        string description,
        Guid createdBy,
        string category = "General",
        WorkflowPriority priority = WorkflowPriority.Normal,
        TimeSpan? defaultTimeout = null,
        int maxConcurrentExecutions = 10,
        bool isTemplate = false,
        Dictionary<string, object>? globalVariables = null) : base(id, createdBy)
    {
        Name = name;
        Description = description;
        Status = WorkflowStatus.Draft;
        Priority = priority;
        Category = category;
        DefaultTimeout = defaultTimeout;
        MaxConcurrentExecutions = maxConcurrentExecutions;
        IsTemplate = isTemplate;
        TotalExecutions = 0;
        GlobalVariables = globalVariables ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new workflow with the specified name and description.
    /// </summary>
    /// <param name="id">The unique identifier for the workflow</param>
    /// <param name="name">The workflow name</param>
    /// <param name="description">The workflow description</param>
    /// <param name="createdBy">The user creating the workflow</param>
    /// <param name="category">The workflow category (optional, defaults to "General")</param>
    /// <param name="priority">The workflow priority (optional, defaults to Normal)</param>
    /// <param name="defaultTimeout">The default timeout for workflow execution</param>
    /// <param name="maxConcurrentExecutions">Maximum concurrent executions allowed</param>
    /// <param name="isTemplate">Whether this workflow is a template</param>
    /// <param name="globalVariables">Global variables for the workflow</param>
    /// <returns>A new WorkflowAggregate instance</returns>
    public static WorkflowAggregate Create(
        Guid id,
        string name,
        string description,
        Guid createdBy,
        string category = "General",
        WorkflowPriority priority = WorkflowPriority.Normal,
        TimeSpan? defaultTimeout = null,
        int maxConcurrentExecutions = 10,
        bool isTemplate = false,
        Dictionary<string, object>? globalVariables = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Workflow ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name cannot be null or empty.", nameof(name));

        if (createdBy == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdBy));

        if (maxConcurrentExecutions <= 0)
            throw new ArgumentException("Maximum concurrent executions must be greater than zero.", nameof(maxConcurrentExecutions));

        var workflow = new WorkflowAggregate(id, name, description, createdBy, category, priority, 
            defaultTimeout, maxConcurrentExecutions, isTemplate, globalVariables);

        workflow.RaiseDomainEvent(new WorkflowCreatedEvent(id, name, description, createdBy));

        return workflow;
    }

    /// <summary>
    /// Updates the workflow's basic information.
    /// </summary>
    /// <param name="name">The new workflow name</param>
    /// <param name="description">The new workflow description</param>
    /// <param name="category">The new workflow category</param>
    /// <param name="priority">The new workflow priority</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateInfo(
        string name,
        string description,
        string category,
        WorkflowPriority priority,
        Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name cannot be null or empty.", nameof(name));

        if (Status == WorkflowStatus.Archived)
            throw new InvalidWorkflowStateException("Cannot update archived workflow.");

        Name = name;
        Description = description;
        Category = category;
        Priority = priority;
        UpdateModified(updatedBy);

        RaiseDomainEvent(new WorkflowUpdatedEvent(Id, name, description, updatedBy));
    }

    /// <summary>
    /// Adds a new node to the workflow.
    /// </summary>
    /// <param name="nodeConfiguration">The configuration for the new node</param>
    /// <param name="positionX">The X position on the canvas</param>
    /// <param name="positionY">The Y position on the canvas</param>
    /// <param name="createdBy">The user adding the node</param>
    /// <param name="displayName">Optional display name for the node</param>
    /// <returns>The created workflow node</returns>
    public WorkflowNode AddNode(
        NodeConfiguration nodeConfiguration,
        double positionX,
        double positionY,
        Guid createdBy,
        string? displayName = null)
    {
        if (Status == WorkflowStatus.Archived)
            throw new InvalidWorkflowStateException("Cannot modify archived workflow.");

        var node = WorkflowNode.Create(Id, nodeConfiguration, positionX, positionY, createdBy, displayName);

        // Validate that the node name is unique within this workflow
        if (_nodes.Any(n => n.DisplayName.Equals(node.DisplayName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleValidationException($"A node with the name '{node.DisplayName}' already exists in this workflow.");
        }

        _nodes.Add(node);
        UpdateModified(createdBy);

        return node;
    }

    /// <summary>
    /// Removes a node from the workflow and all its connections.
    /// </summary>
    /// <param name="nodeId">The ID of the node to remove</param>
    /// <param name="updatedBy">The user removing the node</param>
    public void RemoveNode(Guid nodeId, Guid updatedBy)
    {
        if (Status == WorkflowStatus.Archived)
            throw new InvalidWorkflowStateException("Cannot modify archived workflow.");

        var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null)
            throw new EntityNotFoundException(nameof(WorkflowNode), nodeId);

        // Remove all connections involving this node
        _connections.RemoveAll(c => c.SourceNodeId == nodeId || c.TargetNodeId == nodeId);

        _nodes.Remove(node);
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Adds a connection between two nodes in the workflow.
    /// </summary>
    /// <param name="connection">The connection to add</param>
    /// <param name="updatedBy">The user adding the connection</param>
    public void AddConnection(WorkflowConnection connection, Guid updatedBy)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (Status == WorkflowStatus.Archived)
            throw new InvalidWorkflowStateException("Cannot modify archived workflow.");

        // Validate that both nodes exist in this workflow
        var sourceNode = _nodes.FirstOrDefault(n => n.Id == connection.SourceNodeId);
        var targetNode = _nodes.FirstOrDefault(n => n.Id == connection.TargetNodeId);

        if (sourceNode == null)
            throw new EntityNotFoundException(nameof(WorkflowNode), connection.SourceNodeId);

        if (targetNode == null)
            throw new EntityNotFoundException(nameof(WorkflowNode), connection.TargetNodeId);

        // Check for duplicate connections
        var existingConnection = _connections.FirstOrDefault(c =>
            c.SourceNodeId == connection.SourceNodeId &&
            c.TargetNodeId == connection.TargetNodeId &&
            c.SourcePort == connection.SourcePort &&
            c.TargetPort == connection.TargetPort);

        if (existingConnection != null)
        {
            throw new BusinessRuleValidationException(
                $"Connection already exists between {sourceNode.DisplayName} and {targetNode.DisplayName}.");
        }

        // Validate that this connection won't create a cycle
        if (WouldCreateCycle(connection))
        {
            throw new BusinessRuleValidationException(
                $"Adding connection between {sourceNode.DisplayName} and {targetNode.DisplayName} would create a cycle.");
        }

        _connections.Add(connection);
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Removes a connection between nodes.
    /// </summary>
    /// <param name="sourceNodeId">The source node ID</param>
    /// <param name="targetNodeId">The target node ID</param>
    /// <param name="sourcePort">The source port</param>
    /// <param name="targetPort">The target port</param>
    /// <param name="updatedBy">The user removing the connection</param>
    public void RemoveConnection(
        Guid sourceNodeId,
        Guid targetNodeId,
        string sourcePort,
        string targetPort,
        Guid updatedBy)
    {
        if (Status == WorkflowStatus.Archived)
            throw new InvalidWorkflowStateException("Cannot modify archived workflow.");

        var connection = _connections.FirstOrDefault(c =>
            c.SourceNodeId == sourceNodeId &&
            c.TargetNodeId == targetNodeId &&
            c.SourcePort == sourcePort &&
            c.TargetPort == targetPort);

        if (connection == null)
            throw new EntityNotFoundException("WorkflowConnection", $"{sourceNodeId}->{targetNodeId}");

        _connections.Remove(connection);
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Changes the workflow status with appropriate validation.
    /// </summary>
    /// <param name="newStatus">The new status to set</param>
    /// <param name="changedBy">The user changing the status</param>
    public void ChangeStatus(WorkflowStatus newStatus, Guid changedBy)
    {
        if (Status == newStatus)
            return; // No change needed

        ValidateStatusTransition(Status, newStatus);

        var oldStatus = Status;
        Status = newStatus;
        UpdateModified(changedBy);

        RaiseDomainEvent(new WorkflowStatusChangedEvent(Id, oldStatus, newStatus, changedBy));

        // Raise specific events for important status changes
        if (newStatus == WorkflowStatus.Active)
        {
            RaiseDomainEvent(new WorkflowPublishedEvent(Id, Name, changedBy));
        }
        else if (newStatus == WorkflowStatus.Archived)
        {
            RaiseDomainEvent(new WorkflowArchivedEvent(Id, Name, changedBy, "Workflow archived"));
        }
    }

    /// <summary>
    /// Validates the workflow for execution readiness.
    /// </summary>
    /// <returns>A list of validation errors, empty if validation passes</returns>
    public List<string> ValidateForExecution()
    {
        var errors = new List<string>();

        if (Status != WorkflowStatus.Active)
        {
            errors.Add($"Workflow must be in Active status to execute. Current status: {Status}");
        }

        if (!_nodes.Any())
        {
            errors.Add("Workflow must contain at least one node.");
        }

        // Validate that there's at least one start node (node with no incoming connections)
        var startNodes = _nodes.Where(n => !_connections.Any(c => c.TargetNodeId == n.Id)).ToList();
        if (!startNodes.Any())
        {
            errors.Add("Workflow must have at least one start node (node with no incoming connections).");
        }

        // Validate each node
        foreach (var node in _nodes)
        {
            var nodeErrors = node.Validate();
            errors.AddRange(nodeErrors.Select(e => $"Node '{node.DisplayName}': {e}"));
        }

        // Check for unreachable nodes
        var reachableNodes = GetReachableNodes();
        var unreachableNodes = _nodes.Where(n => !reachableNodes.Contains(n.Id)).ToList();
        foreach (var unreachableNode in unreachableNodes)
        {
            errors.Add($"Node '{unreachableNode.DisplayName}' is not reachable from any start node.");
        }

        return errors;
    }

    /// <summary>
    /// Records that the workflow has been executed.
    /// </summary>
    /// <param name="executionTime">The time when execution started</param>
    public void RecordExecution(DateTime executionTime)
    {
        LastExecutedAt = executionTime;
        TotalExecutions++;
        UpdateModified(CreatedBy); // System update
    }

    /// <summary>
    /// Sets global variables for the workflow.
    /// </summary>
    /// <param name="variables">The global variables to set</param>
    /// <param name="updatedBy">The user updating the variables</param>
    public void SetGlobalVariables(Dictionary<string, object> variables, Guid updatedBy)
    {
        ArgumentNullException.ThrowIfNull(variables);
        GlobalVariables = new Dictionary<string, object>(variables);
        UpdateModified(updatedBy);
    }

    private void ValidateStatusTransition(WorkflowStatus currentStatus, WorkflowStatus newStatus)
    {
        var validTransitions = GetValidStatusTransitions(currentStatus);
        
        if (!validTransitions.Contains(newStatus))
        {
            throw new InvalidWorkflowStateException(
                $"Cannot transition from {currentStatus} to {newStatus}. Valid transitions: {string.Join(", ", validTransitions)}");
        }
    }

    private static List<WorkflowStatus> GetValidStatusTransitions(WorkflowStatus currentStatus)
    {
        return currentStatus switch
        {
            WorkflowStatus.Draft => new List<WorkflowStatus> { WorkflowStatus.Active, WorkflowStatus.Archived },
            WorkflowStatus.Active => new List<WorkflowStatus> { WorkflowStatus.Inactive, WorkflowStatus.Deprecated, WorkflowStatus.Archived },
            WorkflowStatus.Inactive => new List<WorkflowStatus> { WorkflowStatus.Active, WorkflowStatus.Archived },
            WorkflowStatus.Deprecated => new List<WorkflowStatus> { WorkflowStatus.Archived },
            WorkflowStatus.Archived => new List<WorkflowStatus>(), // Archived is final
            _ => new List<WorkflowStatus>()
        };
    }

    private bool WouldCreateCycle(WorkflowConnection newConnection)
    {
        // Create a temporary graph with the new connection
        var tempConnections = new List<WorkflowConnection>(_connections) { newConnection };
        
        // Perform depth-first search from the target node to see if we can reach the source node
        var visited = new HashSet<Guid>();
        return HasPathToNode(newConnection.TargetNodeId, newConnection.SourceNodeId, tempConnections, visited);
    }

    private bool HasPathToNode(Guid from, Guid to, List<WorkflowConnection> connections, HashSet<Guid> visited)
    {
        if (from == to)
            return true;

        if (visited.Contains(from))
            return false;

        visited.Add(from);

        var outgoingConnections = connections.Where(c => c.SourceNodeId == from);
        foreach (var connection in outgoingConnections)
        {
            if (HasPathToNode(connection.TargetNodeId, to, connections, visited))
                return true;
        }

        return false;
    }

    private HashSet<Guid> GetReachableNodes()
    {
        var reachable = new HashSet<Guid>();
        var startNodes = _nodes.Where(n => !_connections.Any(c => c.TargetNodeId == n.Id)).ToList();

        foreach (var startNode in startNodes)
        {
            TraverseReachableNodes(startNode.Id, reachable);
        }

        return reachable;
    }

    private void TraverseReachableNodes(Guid nodeId, HashSet<Guid> visited)
    {
        if (visited.Contains(nodeId))
            return;

        visited.Add(nodeId);

        var outgoingConnections = _connections.Where(c => c.SourceNodeId == nodeId);
        foreach (var connection in outgoingConnections)
        {
            TraverseReachableNodes(connection.TargetNodeId, visited);
        }
    }
}
