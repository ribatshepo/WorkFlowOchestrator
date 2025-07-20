namespace WorkflowPlatform.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class
    /// </summary>
    /// <param name="name">The name of the entity</param>
    /// <param name="key">The key used to search for the entity</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class
    /// </summary>
    /// <param name="message">The exception message</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="failures">The validation failures</param>
    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="message">The exception message</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class
    /// </summary>
    /// <param name="message">The exception message</param>
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
