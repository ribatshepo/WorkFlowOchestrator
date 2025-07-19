namespace WorkflowPlatform.Domain.Common.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions in the workflow platform.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// The error code associated with this domain exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional contextual data related to the exception.
    /// </summary>
    public Dictionary<string, object> Context { get; }

    protected DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    protected DomainException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds contextual information to the exception.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>This exception instance for method chaining</returns>
    public DomainException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated in the domain.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message) 
        : base("BUSINESS_RULE_VIOLATION", message)
    {
    }

    public BusinessRuleValidationException(string message, Exception innerException) 
        : base("BUSINESS_RULE_VIOLATION", message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found in the domain.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityType, object entityId) 
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID '{entityId}' was not found.")
    {
        WithContext("EntityType", entityType)
            .WithContext("EntityId", entityId);
    }
}

/// <summary>
/// Exception thrown when attempting to create an entity that already exists.
/// </summary>
public class EntityAlreadyExistsException : DomainException
{
    public EntityAlreadyExistsException(string entityType, string identifier) 
        : base("ENTITY_ALREADY_EXISTS", $"{entityType} with identifier '{identifier}' already exists.")
    {
        WithContext("EntityType", entityType)
            .WithContext("Identifier", identifier);
    }
}

/// <summary>
/// Exception thrown when a workflow execution fails due to invalid state.
/// </summary>
public class InvalidWorkflowStateException : DomainException
{
    public InvalidWorkflowStateException(string message) 
        : base("INVALID_WORKFLOW_STATE", message)
    {
    }

    public InvalidWorkflowStateException(string currentState, string operation) 
        : base("INVALID_WORKFLOW_STATE", $"Cannot perform '{operation}' operation when workflow is in '{currentState}' state.")
    {
        WithContext("CurrentState", currentState)
            .WithContext("Operation", operation);
    }
}
