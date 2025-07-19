using WorkflowPlatform.Domain.Workflows.Aggregates;

namespace WorkflowPlatform.Application.Common.Interfaces;

/// <summary>
/// Application layer interface for workflow-specific repository operations.
/// This extends the domain repository interface with application-specific queries.
/// </summary>
public interface IWorkflowRepository : WorkflowPlatform.Domain.Common.Interfaces.IRepository<WorkflowAggregate, Guid>
{
    /// <summary>
    /// Gets a workflow by its name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow if found, null otherwise</returns>
    Task<WorkflowAggregate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active workflows.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active workflows</returns>
    Task<IEnumerable<WorkflowAggregate>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflows created by a specific user.
    /// </summary>
    /// <param name="createdBy">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflows created by the user</returns>
    Task<IEnumerable<WorkflowAggregate>> GetByCreatorAsync(Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated workflows with filtering and sorting.
    /// </summary>
    /// <param name="pageSize">Number of workflows per page</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="searchTerm">Optional search term for name/description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result of workflows</returns>
    Task<(IEnumerable<WorkflowAggregate> Workflows, int TotalCount)> GetPaginatedAsync(
        int pageSize = 50, 
        int pageNumber = 1,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
