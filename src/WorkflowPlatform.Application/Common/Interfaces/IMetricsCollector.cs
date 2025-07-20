using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for collecting metrics during node execution
    /// </summary>
    public interface IMetricsCollector
    {
        /// <summary>
        /// Record node execution metrics
        /// </summary>
        void RecordNodeExecution(string nodeType, NodeExecutionStatus status, TimeSpan duration);

        /// <summary>
        /// Record node execution error
        /// </summary>
        void RecordNodeExecutionError(string nodeType, string errorType);

        /// <summary>
        /// Record lifecycle phase duration
        /// </summary>
        void RecordLifecyclePhaseDuration(string nodeType, string phase, TimeSpan duration);

        /// <summary>
        /// Increment counter for node execution attempts
        /// </summary>
        void IncrementNodeExecutionAttempts(string nodeType);

        /// <summary>
        /// Record retry attempt
        /// </summary>
        void RecordRetryAttempt(string nodeType, int attemptNumber);

        /// <summary>
        /// Record circuit breaker state change
        /// </summary>
        void RecordCircuitBreakerStateChange(string nodeType, string state);
    }
}
