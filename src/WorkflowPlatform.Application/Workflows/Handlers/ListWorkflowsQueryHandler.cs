using MediatR;
using WorkflowPlatform.Application.Workflows.Queries.ListWorkflows;
using WorkflowPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using WorkflowPlatform.Application.Common.Models;

namespace WorkflowPlatform.Application.Workflows.Handlers;

/// <summary>
/// Handler for retrieving a paginated list of workflows
/// </summary>
public class ListWorkflowsQueryHandler : IRequestHandler<ListWorkflowsQuery, PaginatedResult<ListWorkflowsResponse>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<ListWorkflowsQueryHandler> _logger;

    public ListWorkflowsQueryHandler(
        IWorkflowRepository workflowRepository,
        Microsoft.Extensions.Logging.ILogger<ListWorkflowsQueryHandler> logger)
    {
        _workflowRepository = workflowRepository!;
        _logger = logger!;
    }

    public async Task<PaginatedResult<ListWorkflowsResponse>> Handle(ListWorkflowsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving workflows - Page: {Page}, Size: {PageSize}, Search: {SearchTerm}",
            request.Page, request.PageSize, request.SearchTerm);

        try
        {
            var (workflows, totalCount) = await _workflowRepository.GetPaginatedAsync(
                pageSize: request.PageSize,
                pageNumber: request.Page,
                searchTerm: request.SearchTerm,
                cancellationToken: cancellationToken);

            var responseItems = workflows.Select(w => new ListWorkflowsResponse
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Version = "1.0.0", 
                IsActive = w.Status == Domain.Common.Enumerations.WorkflowStatus.Active,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.CreatedAt, // Use CreatedAt until UpdatedAt is available in WorkflowAggregate
                Tags = new List<string>(), // Empty until domain model supports tags
                Category = w.Category,
                ExecutionCount = w.TotalExecutions, // Use TotalExecutions from WorkflowAggregate
                LastExecutedAt = w.LastExecutedAt, 
                AverageExecutionTime = null // Will be calculated when metrics are implemented
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} workflows out of {Total} total",
                responseItems.Count, totalCount);

            return new PaginatedResult<ListWorkflowsResponse>
            {
                Items = responseItems,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows list");
            throw;
        }
    }
}