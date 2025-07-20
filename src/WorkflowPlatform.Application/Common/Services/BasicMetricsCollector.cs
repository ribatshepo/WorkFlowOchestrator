using Microsoft.Extensions.Logging;
using System;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Common.Services
{
    /// <summary>
    /// Basic implementation of metrics collector for development/testing
    /// In production, this would integrate with a metrics system like Prometheus
    /// </summary>
    public class BasicMetricsCollector : IMetricsCollector
    {
        private readonly ILogger<BasicMetricsCollector> _logger;

        public BasicMetricsCollector(ILogger<BasicMetricsCollector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void RecordNodeExecution(string nodeType, NodeExecutionStatus status, TimeSpan duration)
        {
            _logger.LogInformation("Node execution metric: {NodeType} completed with {Status} in {Duration}ms",
                nodeType, status, duration.TotalMilliseconds);
        }

        public void RecordNodeExecutionError(string nodeType, string errorType)
        {
            _logger.LogWarning("Node execution error: {NodeType} failed with {ErrorType}",
                nodeType, errorType);
        }

        public void RecordLifecyclePhaseDuration(string nodeType, string phase, TimeSpan duration)
        {
            _logger.LogDebug("Lifecycle phase metric: {NodeType} {Phase} completed in {Duration}ms",
                nodeType, phase, duration.TotalMilliseconds);
        }

        public void IncrementNodeExecutionAttempts(string nodeType)
        {
            _logger.LogDebug("Node execution attempt: {NodeType} execution attempted", nodeType);
        }

        public void RecordRetryAttempt(string nodeType, int attemptNumber)
        {
            _logger.LogInformation("Retry attempt metric: {NodeType} retry attempt #{AttemptNumber}",
                nodeType, attemptNumber);
        }

        public void RecordCircuitBreakerStateChange(string nodeType, string state)
        {
            _logger.LogWarning("Circuit breaker state change: {NodeType} circuit breaker changed to {State}",
                nodeType, state);
        }
    }
}
