using Microsoft.Extensions.Logging;
using System;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Common.Services
{
    /// <summary>
    /// Default implementation of metrics collector for node execution
    /// </summary>
    public class DefaultMetricsCollector : IMetricsCollector
    {
        private readonly ILogger<DefaultMetricsCollector> _logger;

        public DefaultMetricsCollector(ILogger<DefaultMetricsCollector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Record node execution metrics
        /// </summary>
        public void RecordNodeExecution(string nodeType, NodeExecutionStatus status, TimeSpan duration)
        {
            _logger.LogInformation(
                "Node execution recorded - NodeType: {NodeType}, Status: {Status}, Duration: {Duration}ms",
                nodeType, status, duration.TotalMilliseconds);

            // In a production implementation, this would send metrics to:
            // - Prometheus/OpenTelemetry
            // - Application Insights
            // - CloudWatch
            // - Custom metrics backend
        }

        /// <summary>
        /// Record node execution error
        /// </summary>
        public void RecordNodeExecutionError(string nodeType, string errorType)
        {
            _logger.LogWarning(
                "Node execution error recorded - NodeType: {NodeType}, ErrorType: {ErrorType}",
                nodeType, errorType);
        }

        /// <summary>
        /// Record lifecycle phase duration
        /// </summary>
        public void RecordLifecyclePhaseDuration(string nodeType, string phase, TimeSpan duration)
        {
            _logger.LogDebug(
                "Lifecycle phase duration - NodeType: {NodeType}, Phase: {Phase}, Duration: {Duration}ms",
                nodeType, phase, duration.TotalMilliseconds);
        }

        /// <summary>
        /// Increment counter for node execution attempts
        /// </summary>
        public void IncrementNodeExecutionAttempts(string nodeType)
        {
            _logger.LogDebug("Node execution attempt - NodeType: {NodeType}", nodeType);
        }

        /// <summary>
        /// Record retry attempt
        /// </summary>
        public void RecordRetryAttempt(string nodeType, int attemptNumber)
        {
            _logger.LogInformation(
                "Retry attempt recorded - NodeType: {NodeType}, Attempt: {AttemptNumber}",
                nodeType, attemptNumber);
        }

        /// <summary>
        /// Record circuit breaker state change
        /// </summary>
        public void RecordCircuitBreakerStateChange(string nodeType, string state)
        {
            _logger.LogWarning(
                "Circuit breaker state change - NodeType: {NodeType}, State: {State}",
                nodeType, state);
        }
    }
}
