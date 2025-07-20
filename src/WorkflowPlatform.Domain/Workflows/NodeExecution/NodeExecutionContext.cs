using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WorkflowPlatform.Domain.Workflows.NodeExecution
{
    /// <summary>
    /// Context containing all information needed for node execution
    /// </summary>
    public sealed class NodeExecutionContext : IDisposable
    {
        public Guid NodeId { get; }
        public string NodeType { get; }
        public Guid WorkflowId { get; }
        public Guid ExecutionId { get; }
        public object? InputData { get; private set; }
        public Dictionary<string, object> Configuration { get; }
        public Dictionary<string, object> Properties { get; }
        public Dictionary<string, IDisposable> Resources { get; }
        public CancellationToken CancellationToken { get; }
        public DateTime StartedAt { get; }
        public TimeSpan ExecutionDuration => DateTime.UtcNow - StartedAt;

        private bool _disposed;

        public NodeExecutionContext(
            Guid nodeId,
            string nodeType,
            Guid workflowId,
            Guid executionId,
            object? inputData,
            Dictionary<string, object>? configuration = null,
            CancellationToken cancellationToken = default)
        {
            NodeId = nodeId;
            NodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
            WorkflowId = workflowId;
            ExecutionId = executionId;
            InputData = inputData;
            Configuration = configuration ?? new Dictionary<string, object>();
            Properties = new Dictionary<string, object>();
            Resources = new Dictionary<string, IDisposable>();
            CancellationToken = cancellationToken;
            StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Get typed configuration value
        /// </summary>
        public T? GetConfiguration<T>(string key) where T : class
        {
            return Configuration.TryGetValue(key, out var value) ? value as T : null;
        }

        /// <summary>
        /// Get configuration value or default
        /// </summary>
        public T GetConfigurationOrDefault<T>(string key, T defaultValue)
        {
            if (Configuration.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }

        /// <summary>
        /// Set property value
        /// </summary>
        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }

        /// <summary>
        /// Get property value
        /// </summary>
        public T? GetProperty<T>(string key) where T : class
        {
            return Properties.TryGetValue(key, out var value) ? value as T : null;
        }

        /// <summary>
        /// Add a resource that will be disposed when context is disposed
        /// </summary>
        public void AddResource(string key, IDisposable resource)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeExecutionContext));

            if (Resources.TryGetValue(key, out var existing))
            {
                existing.Dispose();
            }

            Resources[key] = resource;
        }

        /// <summary>
        /// Get a resource
        /// </summary>
        public T? GetResource<T>(string key) where T : class
        {
            return Resources.TryGetValue(key, out var resource) ? resource as T : null;
        }

        /// <summary>
        /// Update input data during execution
        /// </summary>
        public void UpdateInputData(object? newInputData)
        {
            InputData = newInputData;
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var resource in Resources.Values)
            {
                try
                {
                    resource.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }

            Resources.Clear();
            _disposed = true;
        }
    }
}
