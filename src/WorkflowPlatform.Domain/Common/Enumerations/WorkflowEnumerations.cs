namespace WorkflowPlatform.Domain.Common.Enumerations;

/// <summary>
/// Represents the current execution status of a workflow instance.
/// </summary>
public enum WorkflowExecutionStatus
{
    /// <summary>
    /// Workflow execution is queued but has not started yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Workflow execution is currently in progress.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Workflow execution has completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Workflow execution has failed due to an error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Workflow execution was cancelled by a user or system.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Workflow execution has been paused and can be resumed.
    /// </summary>
    Paused = 5,

    /// <summary>
    /// Workflow execution timed out and was terminated.
    /// </summary>
    TimedOut = 6,

    /// <summary>
    /// Workflow execution is waiting for external input or approval.
    /// </summary>
    Waiting = 7
}

/// <summary>
/// Represents the execution status of an individual node within a workflow.
/// </summary>
public enum NodeExecutionStatus
{
    /// <summary>
    /// Node has not been processed yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Node is currently being processed.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Node has completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Node execution failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Node execution was skipped due to conditional logic.
    /// </summary>
    Skipped = 4,

    /// <summary>
    /// Node is waiting for retry after a transient failure.
    /// </summary>
    Retrying = 5,

    /// <summary>
    /// Node execution was cancelled.
    /// </summary>
    Cancelled = 6
}

/// <summary>
/// Represents the priority level for workflow execution.
/// Higher priority workflows are executed before lower priority ones.
/// </summary>
public enum WorkflowPriority
{
    /// <summary>
    /// Low priority - executed when system resources are available.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - standard execution queue.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - executed before normal priority workflows.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - executed immediately with dedicated resources.
    /// </summary>
    Critical = 3
}

/// <summary>
/// Represents the publication status of a workflow definition.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// Workflow is in draft state and not available for execution.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Workflow is active and available for execution.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Workflow is inactive and temporarily disabled.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Workflow has been archived and is read-only.
    /// </summary>
    Archived = 3,

    /// <summary>
    /// Workflow has been deprecated and should not be used for new executions.
    /// </summary>
    Deprecated = 4
}
