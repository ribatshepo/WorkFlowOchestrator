using WorkflowPlatform.Domain.Common.Primitives;
using WorkflowPlatform.Domain.Workflows.ValueObjects;

namespace WorkflowPlatform.Domain.Workflows.Entities;

/// <summary>
/// Entity representing a single node in a workflow definition.
/// Each node encapsulates a specific operation or task within the workflow.
/// </summary>
public class WorkflowNode : Entity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public NodeConfiguration Configuration { get; private set; }
    public string DisplayName { get; private set; }
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public bool IsEnabled { get; private set; }
    public Dictionary<string, object> RuntimeContext { get; private set; }

    // Navigation properties
    public IReadOnlyList<WorkflowConnection> IncomingConnections => _incomingConnections.AsReadOnly();
    public IReadOnlyList<WorkflowConnection> OutgoingConnections => _outgoingConnections.AsReadOnly();

    private readonly List<WorkflowConnection> _incomingConnections = new();
    private readonly List<WorkflowConnection> _outgoingConnections = new();

    // Private constructor for EF Core
    private WorkflowNode() : base() 
    {
        Configuration = null!;
        DisplayName = string.Empty;
        RuntimeContext = new Dictionary<string, object>();
    }

    private WorkflowNode(
        Guid id,
        Guid workflowId,
        NodeConfiguration configuration,
        string displayName,
        double positionX,
        double positionY,
        Guid createdBy) : base(id, createdBy)
    {
        WorkflowId = workflowId;
        Configuration = configuration;
        DisplayName = displayName;
        PositionX = positionX;
        PositionY = positionY;
        IsEnabled = true;
        RuntimeContext = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new workflow node with the specified configuration and position.
    /// </summary>
    /// <param name="workflowId">The ID of the workflow this node belongs to</param>
    /// <param name="configuration">The node configuration defining its behavior</param>
    /// <param name="displayName">The display name for the node (optional, uses config name if not provided)</param>
    /// <param name="positionX">The X coordinate position on the workflow canvas</param>
    /// <param name="positionY">The Y coordinate position on the workflow canvas</param>
    /// <param name="createdBy">The user ID who created this node</param>
    /// <returns>A new WorkflowNode instance</returns>
    public static WorkflowNode Create(
        Guid workflowId,
        NodeConfiguration configuration,
        double positionX,
        double positionY,
        Guid createdBy,
        string? displayName = null)
    {
        if (workflowId == Guid.Empty)
            throw new ArgumentException("Workflow ID cannot be empty.", nameof(workflowId));

        ArgumentNullException.ThrowIfNull(configuration);

        if (createdBy == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdBy));

        var nodeId = Guid.NewGuid();
        var effectiveDisplayName = displayName ?? configuration.Name;

        return new WorkflowNode(
            nodeId,
            workflowId,
            configuration,
            effectiveDisplayName,
            positionX,
            positionY,
            createdBy);
    }

    /// <summary>
    /// Updates the node's configuration and display name.
    /// </summary>
    /// <param name="configuration">The new node configuration</param>
    /// <param name="displayName">The new display name (optional)</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateConfiguration(NodeConfiguration configuration, Guid updatedBy, string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = configuration;
        DisplayName = displayName ?? configuration.Name;
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Updates the node's position on the workflow canvas.
    /// </summary>
    /// <param name="positionX">The new X coordinate</param>
    /// <param name="positionY">The new Y coordinate</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdatePosition(double positionX, double positionY, Guid updatedBy)
    {
        PositionX = positionX;
        PositionY = positionY;
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Enables or disables the node for execution.
    /// </summary>
    /// <param name="isEnabled">Whether the node should be enabled</param>
    /// <param name="updatedBy">The user making the change</param>
    public void SetEnabled(bool isEnabled, Guid updatedBy)
    {
        IsEnabled = isEnabled;
        UpdateModified(updatedBy);
    }

    /// <summary>
    /// Adds runtime context data to the node.
    /// This data is used during workflow execution and is not persisted with the definition.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    public void SetRuntimeContext(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        RuntimeContext[key] = value;
    }

    /// <summary>
    /// Gets a runtime context value.
    /// </summary>
    /// <typeparam name="T">The expected type of the context value</typeparam>
    /// <param name="key">The context key</param>
    /// <param name="defaultValue">The default value if the key doesn't exist</param>
    /// <returns>The context value or default value</returns>
    public T GetRuntimeContext<T>(string key, T defaultValue = default!)
    {
        if (RuntimeContext.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Validates the node configuration and connections.
    /// </summary>
    /// <returns>A list of validation errors, empty if validation passes</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Validate the node configuration based on its type
        var requiredProperties = GetRequiredPropertiesForNodeType(Configuration.NodeType);
        errors.AddRange(Configuration.Validate(requiredProperties));

        return errors;
    }

    /// <summary>
    /// Determines if this node can execute based on its incoming connections and their states.
    /// </summary>
    /// <param name="completedNodes">The set of nodes that have completed successfully</param>
    /// <returns>True if the node can execute, false otherwise</returns>
    public bool CanExecute(ISet<Guid> completedNodes)
    {
        if (!IsEnabled)
            return false;

        // If there are no incoming connections, the node can execute (it's a start node)
        if (!_incomingConnections.Any())
            return true;

        // All source nodes of incoming connections must be completed
        return _incomingConnections.All(conn => completedNodes.Contains(conn.SourceNodeId));
    }

    /// <summary>
    /// Gets the required properties for a specific node type.
    /// This would typically be loaded from a configuration or registry.
    /// </summary>
    /// <param name="nodeType">The type of node</param>
    /// <returns>List of required property names</returns>
    private static List<string> GetRequiredPropertiesForNodeType(string nodeType)
    {
        // TODO: This should be loaded from a node type registry or configuration
        // For now, return basic requirements based on common node types
        return nodeType.ToLowerInvariant() switch
        {
            "httprequest" => new List<string> { "url", "method" },
            "databasequery" => new List<string> { "connectionString", "query" },
            "emailnotification" => new List<string> { "to", "subject" },
            _ => new List<string>()
        };
    }
}
