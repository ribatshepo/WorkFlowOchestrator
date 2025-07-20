using System;
using System.Collections.Generic;
using WorkflowPlatform.Domain.Common;

namespace WorkflowPlatform.Domain.Workflows.NodeExecution
{
    /// <summary>
    /// Represents the result of a node execution
    /// </summary>
    public class NodeExecutionResult
    {
        public bool IsSuccess { get; private set; }
        public object? OutputData { get; private set; }
        public string? ErrorMessage { get; private set; }
        public Exception? Exception { get; private set; }
        public NodeExecutionStatus Status { get; private set; }
        public TimeSpan? ExecutionDuration { get; private set; }
        public Dictionary<string, object> Metadata { get; }

        private NodeExecutionResult(
            bool isSuccess,
            NodeExecutionStatus status,
            object? outputData = null,
            string? errorMessage = null,
            Exception? exception = null,
            TimeSpan? executionDuration = null)
        {
            IsSuccess = isSuccess;
            Status = status;
            OutputData = outputData;
            ErrorMessage = errorMessage;
            Exception = exception;
            ExecutionDuration = executionDuration;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create a successful result
        /// </summary>
        public static NodeExecutionResult Success(object? outputData = null, TimeSpan? executionDuration = null)
        {
            return new NodeExecutionResult(true, NodeExecutionStatus.Completed, outputData, executionDuration: executionDuration);
        }

        /// <summary>
        /// Create a failed result
        /// </summary>
        public static NodeExecutionResult Failed(string errorMessage, Exception? exception = null, TimeSpan? executionDuration = null)
        {
            return new NodeExecutionResult(false, NodeExecutionStatus.Failed, errorMessage: errorMessage, exception: exception, executionDuration: executionDuration);
        }

        /// <summary>
        /// Create a failed result from exception
        /// </summary>
        public static NodeExecutionResult Failed(Exception exception, TimeSpan? executionDuration = null)
        {
            return new NodeExecutionResult(false, NodeExecutionStatus.Failed, errorMessage: exception.Message, exception: exception, executionDuration: executionDuration);
        }

        /// <summary>
        /// Create a cancelled result
        /// </summary>
        public static NodeExecutionResult Cancelled(string? reason = null, TimeSpan? executionDuration = null)
        {
            return new NodeExecutionResult(false, NodeExecutionStatus.Cancelled, errorMessage: reason ?? "Operation was cancelled", executionDuration: executionDuration);
        }

        /// <summary>
        /// Create a timeout result
        /// </summary>
        public static NodeExecutionResult Timeout(TimeSpan timeout, TimeSpan? executionDuration = null)
        {
            return new NodeExecutionResult(false, NodeExecutionStatus.TimedOut, errorMessage: $"Operation timed out after {timeout}", executionDuration: executionDuration);
        }

        /// <summary>
        /// Create a retry result
        /// </summary>
        public static NodeExecutionResult Retry(string reason, int attemptNumber, TimeSpan? executionDuration = null)
        {
            var result = new NodeExecutionResult(false, NodeExecutionStatus.Retrying, errorMessage: reason, executionDuration: executionDuration);
            result.Metadata["AttemptNumber"] = attemptNumber;
            return result;
        }

        /// <summary>
        /// Add metadata to the result
        /// </summary>
        public NodeExecutionResult WithMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Add metadata dictionary to the result
        /// </summary>
        public NodeExecutionResult WithMetadata(Dictionary<string, object> metadata)
        {
            foreach (var kvp in metadata)
            {
                Metadata[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Create a new result with updated output data
        /// </summary>
        public NodeExecutionResult WithOutputData(object? outputData)
        {
            var result = new NodeExecutionResult(IsSuccess, Status, outputData, ErrorMessage, Exception, ExecutionDuration);
            foreach (var kvp in Metadata)
            {
                result.Metadata[kvp.Key] = kvp.Value;
            }
            return result;
        }

        public override string ToString()
        {
            return IsSuccess 
                ? $"Success: {Status}" 
                : $"Failed: {Status} - {ErrorMessage}";
        }
    }

    /// <summary>
    /// Node execution status enumeration
    /// </summary>
    public enum NodeExecutionStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        TimedOut = 5,
        Retrying = 6,
        Skipped = 7
    }
}
