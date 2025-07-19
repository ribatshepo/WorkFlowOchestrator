using WorkflowPlatform.Domain.Common.Primitives;

namespace WorkflowPlatform.Domain.Workflows.ValueObjects;

/// <summary>
/// Value object representing a connection between two workflow nodes.
/// Defines the flow of data and control between nodes in a workflow.
/// </summary>
public class WorkflowConnection : ValueObject
{
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public string SourcePort { get; private set; }
    public string TargetPort { get; private set; }
    public string? Condition { get; private set; }
    public Dictionary<string, object> DataMapping { get; private set; }

    private WorkflowConnection(
        Guid sourceNodeId,
        Guid targetNodeId,
        string sourcePort,
        string targetPort,
        string? condition,
        Dictionary<string, object> dataMapping)
    {
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        SourcePort = sourcePort;
        TargetPort = targetPort;
        Condition = condition;
        DataMapping = new Dictionary<string, object>(dataMapping);
    }

    /// <summary>
    /// Creates a new workflow connection between two nodes.
    /// </summary>
    /// <param name="sourceNodeId">The ID of the source node</param>
    /// <param name="targetNodeId">The ID of the target node</param>
    /// <param name="sourcePort">The output port of the source node (default: "output")</param>
    /// <param name="targetPort">The input port of the target node (default: "input")</param>
    /// <param name="condition">Optional condition that must be met for data to flow</param>
    /// <param name="dataMapping">Optional data transformation mapping</param>
    /// <returns>A new WorkflowConnection instance</returns>
    public static WorkflowConnection Create(
        Guid sourceNodeId,
        Guid targetNodeId,
        string sourcePort = "output",
        string targetPort = "input",
        string? condition = null,
        Dictionary<string, object>? dataMapping = null)
    {
        if (sourceNodeId == Guid.Empty)
            throw new ArgumentException("Source node ID cannot be empty.", nameof(sourceNodeId));

        if (targetNodeId == Guid.Empty)
            throw new ArgumentException("Target node ID cannot be empty.", nameof(targetNodeId));

        if (sourceNodeId == targetNodeId)
            throw new ArgumentException("Source and target nodes cannot be the same.");

        if (string.IsNullOrWhiteSpace(sourcePort))
            throw new ArgumentException("Source port cannot be null or empty.", nameof(sourcePort));

        if (string.IsNullOrWhiteSpace(targetPort))
            throw new ArgumentException("Target port cannot be null or empty.", nameof(targetPort));

        return new WorkflowConnection(
            sourceNodeId,
            targetNodeId,
            sourcePort,
            targetPort,
            condition,
            dataMapping ?? new Dictionary<string, object>());
    }

    /// <summary>
    /// Determines if this connection should be followed based on the condition and source data.
    /// </summary>
    /// <param name="sourceData">The output data from the source node</param>
    /// <returns>True if the connection should be followed, false otherwise</returns>
    public bool ShouldFollow(object? sourceData)
    {
        // If no condition is specified, always follow the connection
        if (string.IsNullOrWhiteSpace(Condition))
            return true;

        // TODO: Implement condition evaluation logic
        // This would typically involve parsing and evaluating the condition expression
        // against the source data. For now, return true as a placeholder.
        return true;
    }

    /// <summary>
    /// Transforms the source data according to the data mapping configuration.
    /// </summary>
    /// <param name="sourceData">The data to transform</param>
    /// <returns>The transformed data</returns>
    public object? TransformData(object? sourceData)
    {
        if (sourceData == null || !DataMapping.Any())
            return sourceData;

        // TODO: Implement data transformation logic
        // This would involve applying the mapping rules to transform the source data
        // For now, return the source data unchanged as a placeholder.
        return sourceData;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return SourceNodeId;
        yield return TargetNodeId;
        yield return SourcePort;
        yield return TargetPort;
        yield return Condition;

        // Include data mapping in a deterministic order
        foreach (var kvp in DataMapping.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
