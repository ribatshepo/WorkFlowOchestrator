using WorkflowPlatform.Domain.Common.Primitives;

namespace WorkflowPlatform.Domain.Workflows.ValueObjects;

/// <summary>
/// Value object representing the configuration for a workflow node.
/// Contains all the settings and parameters needed for node execution.
/// </summary>
public class NodeConfiguration : ValueObject
{
    public string NodeType { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Dictionary<string, object> Properties { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }
    public TimeSpan? Timeout { get; private set; }
    public int MaxRetryAttempts { get; private set; }
    public bool ContinueOnFailure { get; private set; }

    private NodeConfiguration(
        string nodeType,
        string name,
        string description,
        Dictionary<string, object> properties,
        Dictionary<string, string> metadata,
        TimeSpan? timeout,
        int maxRetryAttempts,
        bool continueOnFailure)
    {
        NodeType = nodeType;
        Name = name;
        Description = description;
        Properties = new Dictionary<string, object>(properties);
        Metadata = new Dictionary<string, string>(metadata);
        Timeout = timeout;
        MaxRetryAttempts = maxRetryAttempts;
        ContinueOnFailure = continueOnFailure;
    }

    /// <summary>
    /// Creates a new node configuration with validation.
    /// </summary>
    /// <param name="nodeType">The type of node (e.g., HttpRequest, DatabaseQuery)</param>
    /// <param name="name">The display name for the node</param>
    /// <param name="description">Optional description of what the node does</param>
    /// <param name="properties">Node-specific configuration properties</param>
    /// <param name="metadata">Additional metadata for the node</param>
    /// <param name="timeout">Optional timeout for node execution</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default: 3)</param>
    /// <param name="continueOnFailure">Whether to continue workflow execution if this node fails</param>
    /// <returns>A new NodeConfiguration instance</returns>
    public static NodeConfiguration Create(
        string nodeType,
        string name,
        string description = "",
        Dictionary<string, object>? properties = null,
        Dictionary<string, string>? metadata = null,
        TimeSpan? timeout = null,
        int maxRetryAttempts = 3,
        bool continueOnFailure = false)
    {
        if (string.IsNullOrWhiteSpace(nodeType))
            throw new ArgumentException("Node type cannot be null or empty.", nameof(nodeType));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Node name cannot be null or empty.", nameof(name));

        if (maxRetryAttempts < 0)
            throw new ArgumentException("Max retry attempts cannot be negative.", nameof(maxRetryAttempts));

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive if specified.", nameof(timeout));

        return new NodeConfiguration(
            nodeType,
            name,
            description,
            properties ?? new Dictionary<string, object>(),
            metadata ?? new Dictionary<string, string>(),
            timeout,
            maxRetryAttempts,
            continueOnFailure);
    }

    /// <summary>
    /// Gets a strongly-typed property value from the node configuration.
    /// </summary>
    /// <typeparam name="T">The expected type of the property</typeparam>
    /// <param name="key">The property key</param>
    /// <param name="defaultValue">The default value to return if the property doesn't exist</param>
    /// <returns>The property value or the default value</returns>
    public T GetProperty<T>(string key, T defaultValue = default!)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Checks if a required property exists in the configuration.
    /// </summary>
    /// <param name="key">The property key to check</param>
    /// <returns>True if the property exists, false otherwise</returns>
    public bool HasProperty(string key)
    {
        return Properties.ContainsKey(key);
    }

    /// <summary>
    /// Validates the configuration against the node type requirements.
    /// </summary>
    /// <param name="requiredProperties">List of required property keys</param>
    /// <returns>A list of validation errors, empty if validation passes</returns>
    public List<string> Validate(IEnumerable<string> requiredProperties)
    {
        var errors = new List<string>();

        foreach (var required in requiredProperties)
        {
            if (!HasProperty(required))
            {
                errors.Add($"Required property '{required}' is missing for node type '{NodeType}'.");
            }
        }

        return errors;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return NodeType;
        yield return Name;
        yield return Description;
        yield return Timeout;
        yield return MaxRetryAttempts;
        yield return ContinueOnFailure;

        // Include properties in a deterministic order
        foreach (var kvp in Properties.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }

        // Include metadata in a deterministic order
        foreach (var kvp in Metadata.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
