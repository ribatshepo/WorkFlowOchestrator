using MediatR;
using WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;
using WorkflowPlatform.Domain.Workflows.Aggregates;
using WorkflowPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using static WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow.CreateWorkflowCommand;

namespace WorkflowPlatform.Application.Workflows.Handlers;

/// <summary>
/// Handler for creating new workflows
/// </summary>
public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, CreateWorkflowResult>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ILogger<CreateWorkflowCommandHandler> _logger;

    public CreateWorkflowCommandHandler(
        IWorkflowRepository workflowRepository,
        ILogger<CreateWorkflowCommandHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _logger = logger;
    }

    public async Task<CreateWorkflowResult> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating workflow: {WorkflowName}", request.Name);

        try
        {
            // Create domain entity using the aggregate root with all provided properties
            var workflowId = Guid.NewGuid();
            var workflow = WorkflowAggregate.Create(
                id: workflowId,
                name: request.Name,
                description: request.Description,
                createdBy: Guid.NewGuid(), // Will be replaced with actual user context in future iterations
                category: request.Category ?? "General",
                priority: request.Priority ?? Domain.Common.Enumerations.WorkflowPriority.Normal,
                defaultTimeout: request.DefaultTimeout,
                maxConcurrentExecutions: request.MaxConcurrentExecutions ?? 10,
                isTemplate: request.IsTemplate ?? false,
                globalVariables: request.GlobalVariables);

            // Save to repository
            await _workflowRepository.AddAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created workflow with ID: {WorkflowId}", workflow.Id);

            // Return success result
            return CreateWorkflowResult.Success(workflow.Id, workflow.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow: {WorkflowName}", request.Name);
            throw;
        }
    }
}
