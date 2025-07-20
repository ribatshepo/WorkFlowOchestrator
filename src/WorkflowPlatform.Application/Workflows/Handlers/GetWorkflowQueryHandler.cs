using MediatR;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;
using WorkflowPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using WorkflowPlatform.Application.Common.Exceptions;
using static WorkflowPlatform.Application.Workflows.Queries.GetWorkflow.GetWorkflowQuery;

namespace WorkflowPlatform.Application.Workflows.Handlers;

/// <summary>
/// Handler for retrieving a workflow by ID
/// </summary>
public class GetWorkflowQueryHandler : IRequestHandler<GetWorkflowQuery, GetWorkflowResult>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ILogger<GetWorkflowQueryHandler> _logger;

    public GetWorkflowQueryHandler(
        IWorkflowRepository workflowRepository,
        ILogger<GetWorkflowQueryHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _logger = logger;
    }

    public async Task<GetWorkflowResult> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving workflow with ID: {WorkflowId}", request.Id);

        try
        {
            var workflow = await _workflowRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow not found with ID: {WorkflowId}", request.Id);
                return GetWorkflowResult.NotFound(request.Id);
            }

            _logger.LogInformation("Successfully retrieved workflow: {WorkflowName}", workflow.Name);

            return GetWorkflowResult.Success(
                workflowId: workflow.Id,
                name: workflow.Name,
                description: workflow.Description,
                category: workflow.Category,
                status: workflow.Status.ToString(),
                createdAt: workflow.CreatedAt,
                updatedAt: workflow.CreatedAt, // Using CreatedAt until UpdatedAt is available
                createdBy: "System", // Will be replaced with actual user context
                lastModifiedBy: "System", // Will be replaced with actual user context
                version: 1, // Default version until domain model supports versioning
                isTemplate: workflow.IsTemplate,
                globalVariables: workflow.GlobalVariables ?? new Dictionary<string, object>(),
                metadata: new Dictionary<string, string>() // Will be added when domain model supports metadata
            );
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow with ID: {WorkflowId}", request.Id);
            throw;
        }
    }
}
