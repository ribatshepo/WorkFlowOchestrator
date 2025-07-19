# Development Guide

## 🚀 Overview

This guide provides comprehensive development practices, patterns, and workflows for building on the Workflow Platform's Clean Architecture foundation. It covers advanced development techniques, coding standards, and best practices for maintaining enterprise-grade code quality.

## 🎯 Current Status

✅ **Epic WOP-E001.1**: 100% Complete - Development foundation established  
✅ **Clean Architecture**: Full implementation with proper separation of concerns  
✅ **SOLID Principles**: Applied throughout the codebase  
✅ **Development Tools**: Complete development environment setup  

## Development Environment Setup

### Required Tools and Extensions

#### Visual Studio 2022 (Recommended)

**Extensions:**
- **ReSharper** or **Visual Studio IntelliCode**: Enhanced code analysis
- **CodeMaid**: Code cleanup and formatting
- **GitLens**: Advanced Git integration
- **SonarLint**: Real-time code quality feedback
- **Productivity Power Tools**: Enhanced IDE experience

#### VS Code Alternative

**Extensions:**
- **C# Dev Kit**: Full .NET development support
- **C# Extensions**: Additional C# functionality
- **GitLens**: Git supercharged
- **REST Client**: API testing directly in VS Code
- **Thunder Client**: Alternative API testing
- **SonarLint**: Code quality analysis
- **Prettier**: Code formatting
- **Auto Rename Tag**: HTML/XML tag synchronization

### Development Configuration

#### 1. EditorConfig Setup

Create `.editorconfig` in solution root:

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.{js,ts,json,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false

[*.cs]
# Organize usings
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Code style rules
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_init = true
csharp_new_line_before_members_in_anonymous_types = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_indent_labels = flush_left

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_method_call_parameter_list_parentheses = false

# Wrapping preferences
csharp_preserve_single_line_statements = false
csharp_preserve_single_line_blocks = true
```

#### 2. Directory.Build.props Configuration

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.16.0.82469" PrivateAssets="all" />
  </ItemGroup>

  <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DefineConstants>TRACE</DefineConstants>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
  </PropertyGroup>
</Project>
```

#### 3. Global Using Statements

Create `GlobalUsings.cs` in each project:

```csharp
// Domain Project GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// Application Project GlobalUsings.cs  
global using MediatR;
global using FluentValidation;
global using Microsoft.Extensions.Logging;
global using WorkflowPlatform.Domain.Common.Primitives;
global using WorkflowPlatform.Domain.Common.Interfaces;

// Infrastructure Project GlobalUsings.cs
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using WorkflowPlatform.Application.Common.Interfaces;

// API Project GlobalUsings.cs
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using System.ComponentModel.DataAnnotations;
```

## Clean Architecture Development Patterns

### 1. Domain-Driven Design Patterns

#### Entity Development Pattern

**Template for new entities:**

```csharp
/// <summary>
/// Represents a [entity description] in the workflow domain
/// </summary>
public class [EntityName] : Entity<Guid>
{
    #region Private Fields
    private readonly List<[RelatedEntity]> _[relatedEntities] = new();
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public string [PropertyName] { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the [collection description]
    /// </summary>
    public IReadOnlyList<[RelatedEntity]> [RelatedEntities] => _[relatedEntities].AsReadOnly();
    #endregion

    #region Factory Methods
    /// <summary>
    /// Creates a new [entity name] with the specified parameters
    /// </summary>
    /// <param name="[parameter]">[Parameter description]</param>
    /// <returns>A new [entity name] instance</returns>
    /// <exception cref="ArgumentException">Thrown when [parameter] is invalid</exception>
    public static [EntityName] Create([ParameterType] [parameter])
    {
        Guard.Against.NullOrWhiteSpace([parameter], nameof([parameter]));

        return new [EntityName]
        {
            Id = Guid.NewGuid(),
            [PropertyName] = [parameter],
            CreatedAt = DateTime.UtcNow
        };
    }
    #endregion

    #region Business Methods
    /// <summary>
    /// [Business method description]
    /// </summary>
    /// <param name="[parameter]">[Parameter description]</param>
    /// <exception cref="DomainException">Thrown when business rule is violated</exception>
    public void [BusinessMethod]([ParameterType] [parameter])
    {
        Guard.Against.Null([parameter], nameof([parameter]));

        // Business rule validation
        if (![BusinessRule])
            throw new DomainException("[Business rule violation message]");

        // Apply changes
        [PropertyName] = [parameter];
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event if significant
        RaiseDomainEvent(new [EventName](Id, [parameter]));
    }
    #endregion

    #region Private Constructor
    private [EntityName]() { } // EF Core constructor
    #endregion
}
```

#### Value Object Development Pattern

```csharp
/// <summary>
/// Represents [value object description]
/// </summary>
public class [ValueObjectName] : ValueObject
{
    #region Properties
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public [PropertyType] [PropertyName] { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of [ValueObjectName]
    /// </summary>
    /// <param name="[parameter]">[Parameter description]</param>
    public [ValueObjectName]([ParameterType] [parameter])
    {
        Guard.Against.Null([parameter], nameof([parameter]));
        // Additional validation if needed
        
        [PropertyName] = [parameter];
    }
    #endregion

    #region ValueObject Implementation
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return [PropertyName];
        // Add other properties that define equality
    }
    #endregion

    #region Business Methods
    /// <summary>
    /// [Method description]
    /// </summary>
    /// <returns>[Return description]</returns>
    public [ValueObjectName] [Method]([Parameters])
    {
        // Value objects are immutable - return new instance
        return new [ValueObjectName]([NewValues]);
    }
    #endregion

    #region Operators (if applicable)
    public static implicit operator [UnderlyingType]([ValueObjectName] valueObject)
    {
        return valueObject.[PropertyName];
    }

    public static explicit operator [ValueObjectName]([UnderlyingType] value)
    {
        return new [ValueObjectName](value);
    }
    #endregion
}
```

#### Aggregate Root Development Pattern

```csharp
/// <summary>
/// Represents [aggregate description]
/// </summary>
public class [AggregateName] : AggregateRoot<Guid>
{
    #region Private Fields
    private readonly List<[ChildEntity]> _[childEntities] = new();
    #endregion

    #region Public Properties
    public string [PropertyName] { get; private set; } = string.Empty;
    public [ValueObject] [ValueObjectProperty] { get; private set; } = [ValueObject].Default;
    public IReadOnlyList<[ChildEntity]> [ChildEntities] => _[childEntities].AsReadOnly();
    #endregion

    #region Factory Methods
    public static [AggregateName] Create([Parameters])
    {
        Guard.Against.NullOrWhiteSpace([parameter], nameof([parameter]));

        var aggregate = new [AggregateName]
        {
            Id = Guid.NewGuid(),
            [PropertyName] = [parameter]
        };

        // Raise creation event
        aggregate.RaiseDomainEvent(new [AggregateCreatedEvent](
            aggregate.Id, 
            [parameter], 
            DateTime.UtcNow));

        return aggregate;
    }
    #endregion

    #region Business Methods
    /// <summary>
    /// [Business operation description]
    /// </summary>
    public void [BusinessOperation]([Parameters])
    {
        // Validate business rules
        ValidateBusinessRules([parameters]);

        // Apply changes
        [ApplyChanges];

        // Raise domain event
        RaiseDomainEvent(new [OperationEvent](Id, [eventData]));
    }

    /// <summary>
    /// Validates aggregate invariants
    /// </summary>
    public void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace([PropertyName]))
            throw new DomainException("[Property] cannot be empty");

        if (!_[childEntities].Any())
            throw new DomainException("[Aggregate] must contain at least one [child entity]");

        // Additional invariant checks
    }
    #endregion

    #region Private Methods
    private void ValidateBusinessRules([Parameters])
    {
        if (![BusinessRule])
            throw new DomainException("[Business rule violation]");
    }
    #endregion

    #region Private Constructor
    private [AggregateName]() { } // EF Core constructor
    #endregion
}
```

### 2. Application Layer Patterns

#### Command Handler Development Pattern

```csharp
/// <summary>
/// Command to [command description]
/// </summary>
public record [CommandName](
    [PropertyType] [PropertyName]
) : IRequest<[ResultType]>;

/// <summary>
/// Handles [command description]
/// </summary>
public class [CommandName]Handler : IRequestHandler<[CommandName], [ResultType]>
{
    #region Dependencies
    private readonly [IRepository] _[repository];
    private readonly IValidator<[CommandName]> _validator;
    private readonly ILogger<[CommandName]Handler> _logger;
    private readonly [IService] _[service];
    #endregion

    #region Constructor
    public [CommandName]Handler(
        [IRepository] [repository],
        IValidator<[CommandName]> validator,
        ILogger<[CommandName]Handler> logger,
        [IService] [service])
    {
        _[repository] = [repository];
        _validator = validator;
        _logger = logger;
        _[service] = [service];
    }
    #endregion

    #region IRequestHandler Implementation
    public async Task<[ResultType]> Handle(
        [CommandName] request,
        CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity($"{nameof([CommandName])}Handler.Handle");
        activity?.SetTag("[entity].id", request.[PropertyName].ToString());

        using (_logger.BeginScope("Handling {CommandName}", typeof([CommandName]).Name))
        {
            try
            {
                // 1. Validate input
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    _logger.LogWarning("Validation failed for {CommandName}: {Errors}", 
                        typeof([CommandName]).Name, string.Join(", ", errors));
                    return [ResultType].Failure(errors);
                }

                // 2. Check business rules
                var businessValidation = await ValidateBusinessRulesAsync(request, cancellationToken);
                if (!businessValidation.IsValid)
                {
                    return [ResultType].Failure(businessValidation.Errors);
                }

                // 3. Load domain objects
                var [entity] = await Load[Entity]Async(request, cancellationToken);

                // 4. Execute business operation
                [entity].[BusinessMethod](request.[PropertyName]);

                // 5. Persist changes
                await _[repository].UpdateAsync([entity], cancellationToken);

                // 6. Log success
                _logger.LogInformation("Successfully handled {CommandName} for {EntityId}", 
                    typeof([CommandName]).Name, [entity].Id);

                return [ResultType].Success([entity].Id);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation failed for {CommandName}", typeof([CommandName]).Name);
                return [ResultType].Failure(new[] { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle {CommandName}", typeof([CommandName]).Name);
                throw; // Let global exception handler manage
            }
        }
    }
    #endregion

    #region Private Methods
    private async Task<BusinessValidationResult> ValidateBusinessRulesAsync(
        [CommandName] request, 
        CancellationToken cancellationToken)
    {
        // Implement specific business rule validation
        // Example: Check if entity exists, user has permissions, etc.
        
        return BusinessValidationResult.Success();
    }

    private async Task<[EntityType]> Load[Entity]Async(
        [CommandName] request, 
        CancellationToken cancellationToken)
    {
        var [entity] = await _[repository].GetByIdAsync(request.[PropertyName], cancellationToken);
        if ([entity] == null)
        {
            throw new NotFoundException($"[Entity] with id {request.[PropertyName]} not found");
        }

        return [entity];
    }
    #endregion
}
```

#### Query Handler Development Pattern

```csharp
/// <summary>
/// Query to [query description]
/// </summary>
public record [QueryName](
    [PropertyType] [PropertyName]
) : IRequest<[ResultType]>;

/// <summary>
/// Handles [query description]
/// </summary>
public class [QueryName]Handler : IRequestHandler<[QueryName], [ResultType]>
{
    #region Dependencies
    private readonly [IRepository] _[repository];
    private readonly IMapper _mapper;
    private readonly ILogger<[QueryName]Handler> _logger;
    private readonly IMemoryCache _cache;
    #endregion

    #region Constructor
    public [QueryName]Handler(
        [IRepository] [repository],
        IMapper mapper,
        ILogger<[QueryName]Handler> logger,
        IMemoryCache cache)
    {
        _[repository] = [repository];
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }
    #endregion

    #region IRequestHandler Implementation
    public async Task<[ResultType]> Handle(
        [QueryName] request,
        CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity($"{nameof([QueryName])}Handler.Handle");
        activity?.SetTag("query.[property]", request.[PropertyName].ToString());

        try
        {
            // Check cache first (if appropriate)
            var cacheKey = $"[query-prefix]:{request.[PropertyName]}";
            if (_cache.TryGetValue(cacheKey, out [ResultType] cachedResult))
            {
                _logger.LogDebug("Retrieved {QueryName} result from cache", typeof([QueryName]).Name);
                return cachedResult;
            }

            // Execute query
            var [entity] = await _[repository].GetByIdAsync(request.[PropertyName], cancellationToken);
            if ([entity] == null)
            {
                _logger.LogWarning("{EntityType} with id {Id} not found", typeof([EntityType]).Name, request.[PropertyName]);
                return [ResultType].NotFound();
            }

            // Map to DTO
            var dto = _mapper.Map<[DtoType]>([entity]);
            var result = [ResultType].Success(dto);

            // Cache result (with appropriate expiration)
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            _logger.LogDebug("Successfully handled {QueryName}", typeof([QueryName]).Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle {QueryName}", typeof([QueryName]).Name);
            throw;
        }
    }
    #endregion
}
```

### 3. Infrastructure Layer Patterns

#### Repository Implementation Pattern

```csharp
/// <summary>
/// Repository interface for [Entity] operations
/// </summary>
public interface I[Entity]Repository : IRepository<[EntityType], Guid>
{
    /// <summary>
    /// Gets [entity] by [criteria]
    /// </summary>
    Task<[EntityType]?> GetBy[Criteria]Async([CriteriaType] [criteria], CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets [entities] matching [condition]
    /// </summary>
    Task<IEnumerable<[EntityType]>> GetBy[Condition]Async([ConditionType] [condition], CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if [entity] exists with [criteria]
    /// </summary>
    Task<bool> ExistsBy[Criteria]Async([CriteriaType] [criteria], CancellationToken cancellationToken = default);
}

/// <summary>
/// Entity Framework implementation of [Entity] repository
/// </summary>
public class [Entity]Repository : Repository<[EntityType], Guid>, I[Entity]Repository
{
    #region Constructor
    public [Entity]Repository(
        WorkflowDbContext context,
        ILogger<[Entity]Repository> logger)
        : base(context, logger) { }
    #endregion

    #region Custom Query Methods
    public async Task<[EntityType]?> GetBy[Criteria]Async(
        [CriteriaType] [criteria], 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.[RelatedProperty])
            .FirstOrDefaultAsync(e => e.[Property] == [criteria], cancellationToken);
    }

    public async Task<IEnumerable<[EntityType]>> GetBy[Condition]Async(
        [ConditionType] [condition], 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.[Property] == [condition])
            .OrderBy(e => e.[OrderProperty])
            .AsNoTracking() // Use for read-only queries
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBy[Criteria]Async(
        [CriteriaType] [criteria], 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(e => e.[Property] == [criteria], cancellationToken);
    }
    #endregion

    #region Override Base Methods (if needed)
    public override async Task<[EntityType]?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.[RelatedProperty])
            .Include(e => e.[AnotherRelatedProperty])
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    #endregion
}
```

#### Service Implementation Pattern

```csharp
/// <summary>
/// Service interface for [service description]
/// </summary>
public interface I[Service]Service
{
    /// <summary>
    /// [Method description]
    /// </summary>
    Task<[ResultType]> [Method]Async([Parameters], CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of [service description]
/// </summary>
public class [Service]Service : I[Service]Service
{
    #region Dependencies
    private readonly [IDependency] _[dependency];
    private readonly ILogger<[Service]Service> _logger;
    private readonly IOptions<[ConfigurationOptions]> _options;
    #endregion

    #region Constructor
    public [Service]Service(
        [IDependency] [dependency],
        ILogger<[Service]Service> logger,
        IOptions<[ConfigurationOptions]> options)
    {
        _[dependency] = [dependency];
        _logger = logger;
        _options = options;
    }
    #endregion

    #region Interface Implementation
    public async Task<[ResultType]> [Method]Async(
        [Parameters], 
        CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity($"{nameof([Service]Service)}.{nameof([Method])}");
        
        try
        {
            _logger.LogDebug("Starting [method description] for [parameter]", [parameter]);

            // Implementation logic
            var result = await PerformOperationAsync([parameters], cancellationToken);

            _logger.LogInformation("Successfully completed [method description]");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to [method description]");
            throw;
        }
    }
    #endregion

    #region Private Methods
    private async Task<[ResultType]> PerformOperationAsync(
        [Parameters], 
        CancellationToken cancellationToken)
    {
        // Detailed implementation
        await Task.Delay(100, cancellationToken); // Placeholder
        return new [ResultType]();
    }
    #endregion
}
```

## Code Quality Standards

### 1. Naming Conventions

#### Classes and Interfaces

```csharp
// ✅ Good naming
public interface IWorkflowExecutionService { }
public class WorkflowExecutionService : IWorkflowExecutionService { }
public abstract class BaseNodeExecutionStrategy { }
public class HttpRequestNodeStrategy : BaseNodeExecutionStrategy { }

// ❌ Poor naming
public interface WorkflowService { } // Missing 'I' prefix
public class WES { } // Abbreviated
public class Helper { } // Vague
```

#### Methods and Properties

```csharp
// ✅ Good naming
public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(Guid workflowId)
public bool CanExecuteNode(WorkflowNode node)
public DateTime LastExecutionTime { get; private set; }
public IReadOnlyList<ValidationError> ValidationErrors { get; }

// ❌ Poor naming
public async Task<WorkflowExecutionResult> DoStuff(Guid id) // Vague
public bool Check(object obj) // Ambiguous
public DateTime Time { get; set; } // Not specific
public List<object> Data { get; set; } // Generic
```

#### Constants and Fields

```csharp
// ✅ Good naming
public const int MaxRetryAttempts = 3;
public const string DefaultWorkflowStatus = "Draft";
private readonly ILogger<WorkflowService> _logger;
private readonly List<WorkflowNode> _nodes = new();

// ❌ Poor naming
public const int MAX = 3; // Not descriptive
private ILogger log; // Missing prefix and type specification
```

### 2. Method Design Principles

#### Single Responsibility

```csharp
// ✅ Good - Single responsibility
public class WorkflowValidator
{
    public ValidationResult ValidateWorkflow(WorkflowAggregate workflow)
    {
        var errors = new List<string>();

        ValidateWorkflowStructure(workflow, errors);
        ValidateNodeConnections(workflow, errors);
        ValidateBusinessRules(workflow, errors);

        return new ValidationResult(errors);
    }

    private void ValidateWorkflowStructure(WorkflowAggregate workflow, List<string> errors)
    {
        if (!workflow.Nodes.Any(n => n.Type == NodeType.Start))
            errors.Add("Workflow must have a start node");
            
        if (!workflow.Nodes.Any(n => n.Type == NodeType.End))
            errors.Add("Workflow must have at least one end node");
    }
}

// ❌ Poor - Multiple responsibilities
public class WorkflowManager
{
    public async Task<WorkflowResult> ProcessWorkflow(WorkflowRequest request)
    {
        // Validation logic
        if (string.IsNullOrEmpty(request.Name)) return WorkflowResult.Error("Invalid name");
        
        // Database operations
        var existing = await _repository.GetByNameAsync(request.Name);
        
        // Business logic
        var workflow = WorkflowAggregate.Create(request.Name);
        
        // Email notifications
        await _emailService.SendNotificationAsync(workflow);
        
        // Logging
        _logger.LogInformation("Workflow processed");
        
        return WorkflowResult.Success(workflow);
    }
}
```

#### Method Length and Complexity

```csharp
// ✅ Good - Focused and readable
public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(
    Guid workflowId,
    Dictionary<string, object> inputData,
    CancellationToken cancellationToken)
{
    var workflow = await LoadWorkflowAsync(workflowId, cancellationToken);
    ValidateWorkflowExecution(workflow, inputData);
    
    var executionContext = CreateExecutionContext(workflow, inputData);
    var result = await ExecuteNodesSequentiallyAsync(executionContext, cancellationToken);
    
    await SaveExecutionResultAsync(result, cancellationToken);
    
    return result;
}

// Supporting methods keep main method focused
private WorkflowExecutionContext CreateExecutionContext(
    WorkflowAggregate workflow, 
    Dictionary<string, object> inputData)
{
    return new WorkflowExecutionContext(workflow, inputData, Guid.NewGuid());
}

// ❌ Poor - Too long and complex
public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(Guid workflowId)
{
    // 200+ lines of code doing everything in one method
    // Validation, loading, execution, persistence, notifications, etc.
}
```

### 3. Exception Handling Best Practices

#### Specific Exception Types

```csharp
// ✅ Good - Specific exceptions
public class WorkflowValidationException : DomainException
{
    public IEnumerable<string> ValidationErrors { get; }

    public WorkflowValidationException(IEnumerable<string> errors)
        : base($"Workflow validation failed: {string.Join(", ", errors)}")
    {
        ValidationErrors = errors;
    }
}

public class NodeExecutionException : ApplicationException
{
    public Guid NodeId { get; }
    public string NodeName { get; }

    public NodeExecutionException(Guid nodeId, string nodeName, string message, Exception? innerException = null)
        : base($"Node '{nodeName}' (ID: {nodeId}) execution failed: {message}", innerException)
    {
        NodeId = nodeId;
        NodeName = nodeName;
    }
}

// Usage
public async Task<NodeExecutionResult> ExecuteNodeAsync(WorkflowNode node)
{
    try
    {
        return await ExecuteNodeInternalAsync(node);
    }
    catch (HttpRequestException ex)
    {
        throw new NodeExecutionException(node.Id, node.Name, "HTTP request failed", ex);
    }
    catch (TimeoutException ex)
    {
        throw new NodeExecutionException(node.Id, node.Name, "Execution timeout", ex);
    }
}

// ❌ Poor - Generic exceptions
public async Task ExecuteAsync()
{
    try
    {
        // Complex logic
    }
    catch (Exception ex)
    {
        throw new Exception("Something went wrong", ex); // Too generic
    }
}
```

#### Exception Propagation Strategy

```csharp
// ✅ Good - Proper exception handling at each layer

// Domain Layer - Let domain exceptions bubble up
public void UpdateNodeConfiguration(NodeConfiguration configuration)
{
    if (!configuration.IsValidForNodeType(Type))
        throw new DomainException($"Invalid configuration for node type {Type}");
        
    Configuration = configuration;
}

// Application Layer - Convert to application exceptions
public async Task<UpdateNodeResult> Handle(UpdateNodeCommand request)
{
    try
    {
        var node = await _repository.GetByIdAsync(request.NodeId);
        if (node == null)
            return UpdateNodeResult.NotFound();

        node.UpdateConfiguration(request.Configuration);
        await _repository.UpdateAsync(node);
        
        return UpdateNodeResult.Success();
    }
    catch (DomainException ex)
    {
        _logger.LogWarning(ex, "Domain validation failed for node update");
        return UpdateNodeResult.ValidationError(ex.Message);
    }
    // Let other exceptions bubble up to global handler
}

// Presentation Layer - Handle specific exceptions
[HttpPut("{id:guid}")]
public async Task<ActionResult> UpdateNode(Guid id, UpdateNodeRequest request)
{
    var command = new UpdateNodeCommand(id, request.Configuration);
    var result = await _mediator.Send(command);
    
    return result switch
    {
        { IsSuccess: true } => Ok(),
        { IsNotFound: true } => NotFound(),
        { IsValidationError: true } => BadRequest(result.ErrorMessage),
        _ => StatusCode(500, "Internal server error")
    };
}
```

### 4. Performance Optimization Patterns

#### Asynchronous Programming

```csharp
// ✅ Good - Proper async/await usage
public async Task<IEnumerable<WorkflowDto>> GetUserWorkflowsAsync(
    Guid userId, 
    CancellationToken cancellationToken = default)
{
    var workflows = await _repository.GetByUserIdAsync(userId, cancellationToken);
    
    var dtoTasks = workflows.Select(async workflow =>
    {
        var nodeCount = await _nodeRepository.CountByWorkflowIdAsync(workflow.Id, cancellationToken);
        return new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            NodeCount = nodeCount,
            Status = workflow.Status
        };
    });
    
    return await Task.WhenAll(dtoTasks);
}

// ✅ Good - Cancellation support
public async Task<WorkflowExecutionResult> ExecuteWorkflowWithTimeoutAsync(
    Guid workflowId,
    TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);
    
    try
    {
        return await ExecuteWorkflowAsync(workflowId, cts.Token);
    }
    catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
    {
        throw new TimeoutException($"Workflow execution timed out after {timeout}");
    }
}

// ❌ Poor - Blocking async calls
public WorkflowDto GetWorkflow(Guid id)
{
    return GetWorkflowAsync(id).Result; // Potential deadlock
}

// ❌ Poor - Not using cancellation tokens
public async Task ProcessManyWorkflowsAsync(IEnumerable<Guid> workflowIds)
{
    foreach (var id in workflowIds)
    {
        await ProcessWorkflowAsync(id); // No cancellation support
    }
}
```

#### Database Access Optimization

```csharp
// ✅ Good - Efficient querying
public async Task<PagedResult<WorkflowSummaryDto>> GetWorkflowSummariesAsync(
    int pageNumber, 
    int pageSize,
    CancellationToken cancellationToken = default)
{
    var totalCount = await _context.Workflows.CountAsync(cancellationToken);
    
    var workflows = await _context.Workflows
        .AsNoTracking() // Read-only optimization
        .Where(w => !w.IsDeleted)
        .OrderByDescending(w => w.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(w => new WorkflowSummaryDto // Project to DTO in database
        {
            Id = w.Id,
            Name = w.Name,
            Status = w.Status,
            CreatedAt = w.CreatedAt,
            NodeCount = w.Nodes.Count
        })
        .ToListAsync(cancellationToken);
    
    return new PagedResult<WorkflowSummaryDto>(workflows, totalCount, pageNumber, pageSize);
}

// ✅ Good - Batch operations
public async Task UpdateMultipleWorkflowStatusAsync(
    IEnumerable<Guid> workflowIds, 
    WorkflowStatus newStatus,
    CancellationToken cancellationToken = default)
{
    await _context.Workflows
        .Where(w => workflowIds.Contains(w.Id))
        .ExecuteUpdateAsync(
            setters => setters
                .SetProperty(w => w.Status, newStatus)
                .SetProperty(w => w.UpdatedAt, DateTime.UtcNow),
            cancellationToken);
}

// ❌ Poor - N+1 query problem
public async Task<IEnumerable<WorkflowWithNodesDto>> GetWorkflowsWithNodes()
{
    var workflows = await _context.Workflows.ToListAsync();
    
    var result = new List<WorkflowWithNodesDto>();
    foreach (var workflow in workflows)
    {
        // This creates N additional database queries
        var nodes = await _context.Nodes.Where(n => n.WorkflowId == workflow.Id).ToListAsync();
        result.Add(new WorkflowWithNodesDto(workflow, nodes));
    }
    
    return result;
}
```

## Testing Development Patterns

### 1. Unit Test Structure

#### Test Class Organization

```csharp
/// <summary>
/// Unit tests for [ClassUnderTest]
/// </summary>
public class [ClassUnderTest]Tests
{
    #region Test Data Builders
    private static WorkflowAggregate CreateTestWorkflow(
        string name = "Test Workflow",
        WorkflowStatus status = WorkflowStatus.Draft)
    {
        return WorkflowAggregate.Create(name, "Test Description", Guid.NewGuid());
    }
    
    private static WorkflowNode CreateTestNode(
        string name = "Test Node",
        NodeType type = NodeType.Action)
    {
        return WorkflowNode.Create(name, type);
    }
    #endregion

    #region Happy Path Tests
    [Fact]
    public void [MethodName]_With[ValidScenario]_Should[ExpectedBehavior]()
    {
        // Arrange
        var [input] = CreateTest[Input]();
        var [sut] = new [ClassUnderTest]();

        // Act
        var result = [sut].[MethodName]([input]);

        // Assert
        result.Should().NotBeNull();
        result.[Property].Should().Be([expectedValue]);
        // Additional assertions
    }
    #endregion

    #region Error Cases Tests
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void [MethodName]_With[InvalidInput]_Should[ThrowException](string invalidInput)
    {
        // Arrange
        var [sut] = new [ClassUnderTest]();

        // Act & Assert
        var action = () => [sut].[MethodName](invalidInput);
        action.Should().Throw<[ExceptionType]>()
            .WithMessage("*[expectedMessagePart]*");
    }
    #endregion

    #region Edge Cases Tests
    [Fact]
    public void [MethodName]_With[EdgeCaseScenario]_Should[ExpectedBehavior]()
    {
        // Test edge cases like empty collections, boundary values, etc.
    }
    #endregion
}
```

#### Test Method Naming

```csharp
// ✅ Good test names - descriptive and clear
[Fact]
public void Create_WithValidParameters_ShouldCreateWorkflowWithCorrectProperties()

[Fact]
public void AddNode_WithNullNode_ShouldThrowArgumentNullException()

[Theory]
[InlineData(NodeType.Start, NodeType.Action, true)]
[InlineData(NodeType.Action, NodeType.End, true)]
[InlineData(NodeType.End, NodeType.Start, false)]
public void CanConnectTo_WithDifferentNodeTypes_ShouldReturnExpectedResult(
    NodeType fromType, NodeType toType, bool expectedCanConnect)

// ❌ Poor test names - vague and unclear
[Fact]
public void Test1() // No information about what is being tested

[Fact]
public void CreateWorkflow() // Doesn't specify the scenario or expected outcome

[Fact]
public void TestValidation() // Too generic
```

### 2. Integration Test Patterns

#### API Integration Tests

```csharp
public class WorkflowsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WorkflowsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove production database
                services.RemoveDbContext<WorkflowDbContext>();
                
                // Add in-memory database for testing
                services.AddDbContext<WorkflowDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}"));
                
                // Override other services as needed for testing
                services.AddScoped<IEmailService, MockEmailService>();
            });
        });

        _client = _factory.CreateClient();
        
        // Setup authentication for tests
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt());
    }

    [Fact]
    public async Task POST_Workflows_WithValidRequest_ShouldReturnCreatedWorkflow()
    {
        // Arrange
        var request = new CreateWorkflowRequest(
            Name: "Integration Test Workflow",
            Description: "Created during integration testing");

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdWorkflow = await response.Content.ReadFromJsonAsync<CreateWorkflowResponse>();
        createdWorkflow.Should().NotBeNull();
        createdWorkflow!.Name.Should().Be(request.Name);
        
        // Verify resource was created
        response.Headers.Location.Should().NotBeNull();
        var locationResponse = await _client.GetAsync(response.Headers.Location);
        locationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_Workflows_Id_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/workflows/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private string GenerateTestJwt()
    {
        // Generate a valid JWT for testing
        // Implementation depends on your JWT setup
        return "test-jwt-token";
    }
}
```

### 3. Mock and Stub Patterns

#### Repository Mocking

```csharp
public class WorkflowServiceTests
{
    private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
    private readonly Mock<INodeRepository> _nodeRepositoryMock;
    private readonly Mock<ILogger<WorkflowService>> _loggerMock;
    private readonly WorkflowService _sut;

    public WorkflowServiceTests()
    {
        _workflowRepositoryMock = new Mock<IWorkflowRepository>();
        _nodeRepositoryMock = new Mock<INodeRepository>();
        _loggerMock = new Mock<ILogger<WorkflowService>>();
        
        _sut = new WorkflowService(
            _workflowRepositoryMock.Object,
            _nodeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkflowWithNodesAsync_WithExistingWorkflow_ShouldReturnWorkflowWithNodes()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflow = CreateTestWorkflow(workflowId);
        var nodes = new[] { CreateTestNode(), CreateTestNode() };

        _workflowRepositoryMock
            .Setup(r => r.GetByIdAsync(workflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        _nodeRepositoryMock
            .Setup(r => r.GetByWorkflowIdAsync(workflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodes);

        // Act
        var result = await _sut.GetWorkflowWithNodesAsync(workflowId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(workflowId);
        result.Nodes.Should().HaveCount(2);
        
        // Verify all mocks were called as expected
        _workflowRepositoryMock.Verify(
            r => r.GetByIdAsync(workflowId, It.IsAny<CancellationToken>()),
            Times.Once);
        
        _nodeRepositoryMock.Verify(
            r => r.GetByWorkflowIdAsync(workflowId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkflowWithNodesAsync_WithNonExistentWorkflow_ShouldReturnNull()
    {
        // Arrange
        var workflowId = Guid.NewGuid();

        _workflowRepositoryMock
            .Setup(r => r.GetByIdAsync(workflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowAggregate?)null);

        // Act
        var result = await _sut.GetWorkflowWithNodesAsync(workflowId);

        // Assert
        result.Should().BeNull();
        
        // Verify node repository was not called
        _nodeRepositoryMock.Verify(
            r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

## Development Workflow

### 1. Feature Development Process

#### Branch Strategy

```bash
# Create feature branch from main
git checkout main
git pull origin main
git checkout -b feature/WOP-123-add-node-validation

# Work on feature with atomic commits
git add .
git commit -m "feat(domain): add node validation rules"
git commit -m "feat(application): implement node validation command"
git commit -m "feat(api): add node validation endpoint"
git commit -m "test: add node validation tests"
git commit -m "docs: update node validation documentation"
```

#### Commit Message Convention

```bash
# Format: type(scope): description
# Types: feat, fix, docs, style, refactor, test, chore

# ✅ Good commit messages
feat(domain): add workflow execution timeout capability
fix(api): resolve null reference in workflow controller
docs(architecture): update clean architecture documentation
test(application): add workflow command handler tests
refactor(infrastructure): optimize database query performance

# ❌ Poor commit messages
"Fixed bug" # Not descriptive
"WIP" # Work in progress - should not be committed
"Changes" # Too vague
```

### 2. Code Review Checklist

#### Pre-Review Checklist (Author)

- [ ] **Tests**: All new functionality has unit tests with >80% coverage
- [ ] **Integration Tests**: Public APIs have integration tests
- [ ] **Documentation**: Public APIs are documented with XML comments
- [ ] **Architecture**: Changes follow Clean Architecture principles
- [ ] **SOLID Principles**: Code adheres to SOLID principles
- [ ] **Naming**: All classes, methods, and variables have clear, descriptive names
- [ ] **Exception Handling**: Proper exception handling and logging in place
- [ ] **Performance**: No obvious performance issues (N+1 queries, blocking calls, etc.)
- [ ] **Security**: Input validation and authorization checks implemented
- [ ] **Configuration**: No hard-coded values, proper configuration management

#### Review Checklist (Reviewer)

```csharp
// Review Areas:

// 1. Architecture Compliance
// ✅ Check: Proper layer separation
public class WorkflowController : ApiControllerBase // ✅ In Presentation layer
{
    private readonly ISender _mediator; // ✅ Uses Application layer through abstraction
    
    // ❌ Would be wrong: Direct repository injection
    // private readonly IWorkflowRepository _repository;
}

// 2. Domain Logic Location
// ✅ Check: Business logic in domain
public class WorkflowAggregate : AggregateRoot<Guid>
{
    public void ValidateForExecution() // ✅ Business logic in domain
    {
        if (!Nodes.Any(n => n.Type == NodeType.Start))
            throw new DomainException("Workflow must have a start node");
    }
}

// 3. Error Handling
// ✅ Check: Proper exception handling
public async Task<WorkflowResult> Handle(CreateWorkflowCommand request)
{
    try
    {
        // Implementation
    }
    catch (DomainException ex)
    {
        _logger.LogWarning(ex, "Domain validation failed"); // ✅ Proper logging
        return WorkflowResult.ValidationError(ex.Message);
    }
    // ❌ Would be wrong: catch (Exception) without re-throwing
}

// 4. Async/Await Usage
// ✅ Check: Proper async patterns
public async Task<WorkflowDto> GetWorkflowAsync(Guid id, CancellationToken cancellationToken)
{
    return await _repository.GetByIdAsync(id, cancellationToken); // ✅ Passes cancellation token
}
```

### 3. Debugging Techniques

#### Structured Logging for Debugging

```csharp
public class WorkflowExecutionService
{
    private readonly ILogger<WorkflowExecutionService> _logger;

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        Guid workflowId, 
        Dictionary<string, object> inputData,
        CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity("WorkflowExecution");
        activity?.SetTag("workflow.id", workflowId.ToString());
        activity?.SetTag("input.count", inputData.Count.ToString());

        using (_logger.BeginScope("WorkflowExecution {WorkflowId}", workflowId))
        {
            _logger.LogInformation("Starting workflow execution with {InputCount} input parameters", 
                inputData.Count);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Log each major step
                _logger.LogDebug("Loading workflow definition");
                var workflow = await _repository.GetByIdAsync(workflowId, cancellationToken);
                
                _logger.LogDebug("Validating workflow structure");
                workflow.ValidateForExecution();
                
                _logger.LogDebug("Starting node execution");
                var executionResult = await ExecuteNodesAsync(workflow, inputData, cancellationToken);
                
                stopwatch.Stop();
                _logger.LogInformation("Completed workflow execution in {ElapsedMs}ms with status {Status}", 
                    stopwatch.ElapsedMilliseconds, executionResult.Status);

                return executionResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Workflow execution failed after {ElapsedMs}ms", 
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
```

#### Development-Time Debugging Tools

```csharp
// Conditional debugging helpers
public static class DebugHelper
{
    [Conditional("DEBUG")]
    public static void LogObject<T>(T obj, string? prefix = null, [CallerMemberName] string? memberName = null)
    {
        var message = prefix ?? $"Debug from {memberName}";
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine($"{message}: {json}");
    }

    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            System.Diagnostics.Debug.WriteLine($"ASSERTION FAILED: {message}");
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}

// Usage in development
public async Task<NodeExecutionResult> ExecuteNodeAsync(WorkflowNode node)
{
    DebugHelper.LogObject(node, "Executing node");
    
    var result = await ExecuteNodeInternal(node);
    
    DebugHelper.Assert(result != null, "Node execution should never return null");
    DebugHelper.LogObject(result, "Node execution result");
    
    return result;
}
```

## Next Steps

### 1. Advanced Development Topics

- **[Testing Guide](./testing-guide.md)**: Comprehensive testing strategies and patterns
- **[Security Guide](./security-guide.md)**: Security implementation best practices  
- **[Performance Guide](./performance-guide.md)**: Performance optimization techniques

### 2. Specialized Development Areas

- **Event Sourcing Implementation**: Building audit trails with domain events
- **CQRS Advanced Patterns**: Optimizing read/write operations
- **Background Processing**: Implementing reliable background job processing
- **API Versioning**: Managing API evolution strategies

### 3. Tools and Automation

- **CI/CD Pipeline Development**: Automated build and deployment
- **Code Quality Gates**: Automated quality checks and metrics
- **Performance Monitoring**: Application performance monitoring setup
- **Documentation Automation**: Automated API documentation generation

---

**Development Status**: ✅ **PRODUCTION-READY**  
**Code Quality**: 🏆 **ENTERPRISE-GRADE**  
**Architecture**: 🏗️ **CLEAN ARCHITECTURE**  
**Next Guide**: 🧪 **[Testing Guide](./testing-guide.md)**
