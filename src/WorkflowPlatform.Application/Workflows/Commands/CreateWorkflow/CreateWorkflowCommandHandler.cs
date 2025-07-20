using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.Aggregates;

namespace WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Handler for CreateWorkflowCommand.
/// Implements the business logic for creating new workflow definitions.
/// </summary>
public sealed class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, CreateWorkflowResult>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateWorkflowCommand> _validator;
    private readonly ILogger<CreateWorkflowCommandHandler> _logger;

    public CreateWorkflowCommandHandler(
        IWorkflowRepository workflowRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateWorkflowCommand> validator,
        ILogger<CreateWorkflowCommandHandler> logger)
    {
        _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateWorkflowResult> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating workflow with name {WorkflowName}", request.Name);

            // Validate the command
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                _logger.LogWarning("Validation failed for workflow creation: {Errors}", string.Join(", ", errors));
                return CreateWorkflowResult.ValidationFailure(errors);
            }

            // Get current user
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                _logger.LogWarning("Cannot create workflow: No authenticated user found");
                return CreateWorkflowResult.Failure("User authentication required");
            }

            // Check if workflow with same name already exists
            var existingWorkflow = await _workflowRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingWorkflow != null)
            {
                _logger.LogWarning("Workflow with name {WorkflowName} already exists", request.Name);
                return CreateWorkflowResult.Failure($"A workflow with the name '{request.Name}' already exists");
            }

            // Create the workflow aggregate
            var workflow = WorkflowAggregate.Create(
                id: Guid.NewGuid(),
                name: request.Name,
                description: request.Description,
                createdBy: currentUserId.Value,
                category: request.Category ?? "General",
                priority: request.Priority ?? Domain.Common.Enumerations.WorkflowPriority.Normal,
                defaultTimeout: request.DefaultTimeout,
                maxConcurrentExecutions: request.MaxConcurrentExecutions ?? 10,
                isTemplate: request.IsTemplate ?? false,
                globalVariables: request.GlobalVariables);

            // Persist the workflow
            await _workflowRepository.AddAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created workflow {WorkflowId} with name {WorkflowName}", 
                workflow.Id, workflow.Name);

            return CreateWorkflowResult.Success(workflow.Id, workflow.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workflow with name {WorkflowName}", request.Name);
            return CreateWorkflowResult.Failure("An unexpected error occurred while creating the workflow");
        }
    }
}
