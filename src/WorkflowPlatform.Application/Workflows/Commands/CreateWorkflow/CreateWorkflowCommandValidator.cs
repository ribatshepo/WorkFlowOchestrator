using FluentValidation;

namespace WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Validator for CreateWorkflowCommand.
/// Implements comprehensive validation rules for workflow creation.
/// </summary>
public sealed class CreateWorkflowCommandValidator : AbstractValidator<CreateWorkflowCommand>
{
    public CreateWorkflowCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Workflow name is required.")
            .Length(3, 255)
            .WithMessage("Workflow name must be between 3 and 255 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
            .WithMessage("Workflow name can only contain letters, numbers, spaces, hyphens, underscores, and periods.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required.")
            .Length(1, 100)
            .WithMessage("Category must be between 1 and 100 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
            .WithMessage("Category can only contain letters, numbers, spaces, hyphens, and underscores.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid priority value.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.DefaultTimeout)
            .Must(timeout => timeout!.Value > TimeSpan.Zero)
            .WithMessage("Default timeout must be greater than zero.")
            .Must(timeout => timeout!.Value <= TimeSpan.FromHours(24))
            .WithMessage("Default timeout cannot exceed 24 hours.")
            .When(x => x.DefaultTimeout.HasValue);

        RuleFor(x => x.MaxConcurrentExecutions)
            .GreaterThan(0)
            .WithMessage("Maximum concurrent executions must be greater than zero.")
            .LessThanOrEqualTo(1000)
            .WithMessage("Maximum concurrent executions cannot exceed 1000.")
            .When(x => x.MaxConcurrentExecutions.HasValue);

        RuleFor(x => x.GlobalVariables)
            .Must(variables => variables!.Count <= 100)
            .WithMessage("Cannot exceed 100 global variables.")
            .When(x => x.GlobalVariables != null);

        // Validate global variable names
        RuleForEach(x => x.GlobalVariables!.Keys)
            .Must(key => !string.IsNullOrWhiteSpace(key))
            .WithMessage("Global variable names cannot be null or whitespace.")
            .Must(key => key.Length <= 50)
            .WithMessage("Global variable names cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Global variable names must start with a letter and contain only letters, numbers, and underscores.")
            .When(x => x.GlobalVariables?.Any() is true);
    }
}
