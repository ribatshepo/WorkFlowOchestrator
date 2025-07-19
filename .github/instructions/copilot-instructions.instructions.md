---
applyTo: '**'
---

## 🎯 Primary Objectives

GitHub Copilot should assist in generating production-ready, secure, and maintainable code that adheres to enterprise standards for the Workflow Orchestration Platform project.

---

## 🛡️ Security Guardrails

### 1. Authentication & Authorization

**✅ ALWAYS Include:**
```csharp
// Proper JWT validation
[Authorize]
[RequiresPermission("workflow.execute")]
public async Task<IActionResult> ExecuteWorkflow([FromRoute] Guid workflowId)
{
    // Implementation
}

// Input validation
public class CreateWorkflowRequest
{
    [Required, StringLength(255, MinimumLength = 3)]
    public string Name { get; set; }
    
    [Required]
    public string Definition { get; set; }
}
```

**❌ NEVER Generate:**
```csharp
// Insecure endpoints without authorization
public async Task<IActionResult> ExecuteWorkflow()
{
    // No authorization check
}

// Direct database queries without parameterization
var sql = $"SELECT * FROM workflows WHERE name = '{name}'";
```

### 2. Data Validation & Sanitization

**✅ ALWAYS Include:**
```csharp
// Comprehensive input validation
public class NodeExecutionValidator : AbstractValidator<NodeExecutionRequest>
{
    public NodeExecutionValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .Must(BeValidGuid);
            
        RuleFor(x => x.InputData)
            .Must(BeValidJson)
            .WithMessage("Input data must be valid JSON");
    }
}

// Sanitized logging
_logger.LogInformation("Executing workflow {WorkflowId} for user {UserId}", 
    workflowId, userId.ToString("N")[..8] + "****");
```

**❌ NEVER Generate:**
```csharp
// Logging sensitive information
_logger.LogInformation("User password: {Password}", request.Password);

// Unvalidated input processing
var result = JsonConvert.DeserializeObject(untrustedInput);
```

### 3. Error Handling & Information Disclosure

**✅ ALWAYS Include:**
```csharp
// Secure error handling
try
{
    await ExecuteWorkflowAsync(workflowId);
}
catch (ValidationException ex)
{
    return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
}
catch (UnauthorizedAccessException)
{
    return Forbid();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to execute workflow {WorkflowId}", workflowId);
    return StatusCode(500, new { Message = "Internal server error" });
}
```

**❌ NEVER Generate:**
```csharp
// Exposing internal details
catch (Exception ex)
{
    return BadRequest(ex.ToString()); // Exposes stack trace
}

// Generic catch-all without logging
catch { return Ok(); }
```

---

## 🏗️ Architecture Guardrails

### 1. Clean Architecture Compliance

**✅ ALWAYS Follow:**
```csharp
// Domain layer - No external dependencies
public class WorkflowAggregate : AggregateRoot<Guid>
{
    private readonly List<WorkflowNode> _nodes = new();
    
    public static WorkflowAggregate Create(string name, string description, Guid createdBy)
    {
        var workflow = new WorkflowAggregate();
        workflow.Apply(new WorkflowCreatedEvent(Guid.NewGuid(), name, description, createdBy));
        return workflow;
    }
}

// Application layer - Business logic coordination
public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowCommand, WorkflowExecutionResult>
{
    private readonly IWorkflowRepository _repository;
    private readonly IExecutionEngine _engine;
    
    public async Task<WorkflowExecutionResult> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.WorkflowId);
        return await _engine.ExecuteAsync(workflow, request.InputData, cancellationToken);
    }
}
```

**❌ NEVER Generate:**
```csharp
// Domain entities with infrastructure dependencies
public class WorkflowAggregate
{
    private readonly IDbContext _dbContext; // ❌ Infrastructure dependency in domain
    
    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync(); // ❌ Persistence logic in domain
    }
}
```

### 2. Strategy Pattern Implementation

**✅ ALWAYS Follow Node Execution Pattern:**
```csharp
public class HttpRequestNodeStrategy : BaseNodeExecutionStrategy
{
    public override string NodeType => "HttpRequest";
    
    public override async Task<NodeExecutionResult> PreprocessAsync(
        NodeExecutionContext context, CancellationToken cancellationToken)
    {
        // Validate HTTP configuration
        var config = context.GetConfiguration<HttpRequestConfig>();
        if (string.IsNullOrEmpty(config.Url) || !Uri.IsWellFormedUriString(config.Url, UriKind.Absolute))
            return NodeExecutionResult.Failed("Invalid URL configuration");
            
        // Setup HTTP client with timeout and retry policies
        await SetupHttpClientAsync(context, cancellationToken);
        return NodeExecutionResult.Success();
    }
    
    public override async Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var httpClient = context.GetService<HttpClient>();
        var config = context.GetConfiguration<HttpRequestConfig>();
        
        try
        {
            var response = await httpClient.SendAsync(
                CreateHttpRequest(config, context.InputData), 
                cancellationToken);
                
            return NodeExecutionResult.Success(await response.Content.ReadAsStringAsync());
        }
        catch (TaskCanceledException)
        {
            return NodeExecutionResult.Failed("Request timeout");
        }
        catch (HttpRequestException ex)
        {
            return NodeExecutionResult.Failed($"HTTP error: {ex.Message}");
        }
    }
    
    // Override other lifecycle methods...
}
```

### 3. Dependency Injection Patterns

**✅ ALWAYS Register Services Properly:**
```csharp
// In Program.cs or Startup.cs
services.AddScoped<IWorkflowRepository, WorkflowRepository>();
services.AddScoped<IExecutionEngine, WorkflowExecutionEngine>();

// Strategy pattern registration
services.AddScoped<INodeExecutionStrategy, HttpRequestNodeStrategy>();
services.AddScoped<INodeExecutionStrategy, DatabaseQueryNodeStrategy>();
services.AddScoped<INodeExecutionStrategy, EmailNotificationNodeStrategy>();

// Strategy factory
services.AddScoped<INodeStrategyFactory>(provider =>
{
    var strategies = provider.GetServices<INodeExecutionStrategy>();
    return new NodeStrategyFactory(strategies);
});
```

---

## 🚀 Performance Guardrails

### 1. Asynchronous Programming

**✅ ALWAYS Use Async/Await Properly:**
```csharp
// Correct async implementation
public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(
    Guid workflowId, 
    CancellationToken cancellationToken = default)
{
    var workflow = await _repository.GetByIdAsync(workflowId, cancellationToken);
    
    var tasks = workflow.Nodes
        .Where(n => n.CanExecuteParallel)
        .Select(node => ExecuteNodeAsync(node, cancellationToken));
        
    var results = await Task.WhenAll(tasks);
    return AggregateResults(results);
}

// Proper cancellation handling
public async Task<NodeExecutionResult> ExecuteWithTimeoutAsync(
    NodeExecutionContext context, 
    TimeSpan timeout)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
    cts.CancelAfter(timeout);
    
    try
    {
        return await ExecuteAsync(context, cts.Token);
    }
    catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
    {
        return NodeExecutionResult.Failed("Execution timeout");
    }
}
```

**❌ NEVER Generate:**
```csharp
// Blocking async calls
public WorkflowExecutionResult ExecuteWorkflow(Guid workflowId)
{
    return ExecuteWorkflowAsync(workflowId).Result; // ❌ Deadlock risk
}

// Fire-and-forget without proper handling
Task.Run(() => SomeOperation()); // ❌ No error handling or cancellation
```

### 2. Resource Management

**✅ ALWAYS Implement Proper Disposal:**
```csharp
public class HttpRequestNodeStrategy : BaseNodeExecutionStrategy, IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;
    
    protected override async Task FinalizationAsync(
        NodeExecutionContext context, 
        NodeExecutionResult result, 
        CancellationToken cancellationToken)
    {
        await base.FinalizationAsync(context, result, cancellationToken);
        
        // Cleanup resources
        context.Resources?.Dispose();
        
        // Update metrics
        _metrics.RecordExecutionTime(context.NodeType, context.ExecutionDuration);
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
```

### 3. Database Operations

**✅ ALWAYS Optimize Database Access:**
```csharp
// Efficient Entity Framework usage
public async Task<IEnumerable<WorkflowInstance>> GetActiveWorkflowsAsync(
    int pageSize = 50, 
    int pageNumber = 1,
    CancellationToken cancellationToken = default)
{
    return await _context.WorkflowInstances
        .Where(w => w.Status == WorkflowStatus.Running)
        .Include(w => w.WorkflowDefinition)
        .OrderBy(w => w.StartedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking() // Read-only optimization
        .ToListAsync(cancellationToken);
}

// Batch operations for performance
public async Task UpdateNodeStatusesAsync(
    IEnumerable<NodeStatusUpdate> updates,
    CancellationToken cancellationToken = default)
{
    const int batchSize = 100;
    var batches = updates.Chunk(batchSize);
    
    foreach (var batch in batches)
    {
        _context.NodeExecutions.UpdateRange(batch.Select(CreateNodeExecution));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## 🧪 Testing Guardrails

### 1. Unit Test Structure

**✅ ALWAYS Generate Comprehensive Tests:**
```csharp
[Fact]
public async Task ExecuteAsync_WithValidHttpRequest_ShouldReturnSuccessResult()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var strategy = new HttpRequestNodeStrategy(mockHttpClient.Object, _logger, _metrics);
    var context = CreateValidExecutionContext();
    
    var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("{\"result\": \"success\"}")
    };
    
    mockHttpClient
        .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResponse);
    
    // Act
    var result = await strategy.ExecuteAsync(context, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.OutputData.Should().Contain("success");
    mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
}

[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("invalid-url")]
public async Task PreprocessAsync_WithInvalidUrl_ShouldReturnFailure(string invalidUrl)
{
    // Arrange
    var strategy = new HttpRequestNodeStrategy(_httpClient, _logger, _metrics);
    var context = CreateExecutionContextWithUrl(invalidUrl);
    
    // Act
    var result = await strategy.PreprocessAsync(context, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.ErrorMessage.Should().Contain("Invalid URL");
}
```

### 2. Integration Test Patterns

**✅ ALWAYS Include Integration Tests:**
```csharp
public class WorkflowExecutionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public WorkflowExecutionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace with test database
                services.RemoveDbContext<WorkflowDbContext>();
                services.AddDbContext<WorkflowDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }
    
    [Fact]
    public async Task POST_ExecuteWorkflow_WithValidWorkflow_ShouldReturnSuccess()
    {
        // Arrange
        var workflow = await CreateTestWorkflowAsync();
        var request = new { InputData = new { value = "test" } };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/workflows/{workflow.Id}/execute", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WorkflowExecutionResult>();
        result.Status.Should().Be(ExecutionStatus.Completed);
    }
}
```

---

## 🎨 Frontend Guardrails

### 1. React Component Patterns

**✅ ALWAYS Follow Modern React Patterns:**
```typescript
// Proper TypeScript interfaces
interface WorkflowNodeProps {
  node: WorkflowNode;
  isSelected: boolean;
  onNodeUpdate: (nodeId: string, data: Partial<WorkflowNode>) => void;
  onNodeDelete: (nodeId: string) => void;
}

// Proper error boundaries and loading states
export const WorkflowDesigner: React.FC<WorkflowDesignerProps> = ({ workflowId }) => {
  const { data: workflow, isLoading, error } = useWorkflowQuery(workflowId);
  const updateWorkflow = useWorkflowMutation();
  
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error.message} />;
  if (!workflow) return <NotFound />;
  
  const handleNodeAdd = useCallback((nodeType: string, position: Position) => {
    const newNode = createNode(nodeType, position);
    updateWorkflow.mutate({
      ...workflow,
      nodes: [...workflow.nodes, newNode]
    });
  }, [workflow, updateWorkflow]);
  
  return (
    <div className="workflow-designer" data-testid="workflow-designer">
      <ReactFlow
        nodes={workflow.nodes}
        edges={workflow.edges}
        onNodesChange={handleNodesChange}
        onEdgesChange={handleEdgesChange}
        onConnect={handleConnect}
      >
        <Background />
        <Controls />
        <MiniMap />
      </ReactFlow>
    </div>
  );
};
```

**❌ NEVER Generate:**
```typescript
// Untyped props
export const WorkflowNode = ({ node, ...props }) => {
  // Missing TypeScript types
}

// Direct DOM manipulation in React
useEffect(() => {
  document.getElementById('node-' + node.id).style.color = 'red';
}, []);

// Unhandled async operations
const handleSave = () => {
  saveWorkflow(workflow); // No error handling
};
```

### 2. State Management Patterns

**✅ ALWAYS Use Proper State Management:**
```typescript
// Zustand store with TypeScript
interface WorkflowStore {
  currentWorkflow: WorkflowDefinition | null;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  isDirty: boolean;
  
  // Actions
  setCurrentWorkflow: (workflow: WorkflowDefinition | null) => void;
  addNode: (node: WorkflowNode) => void;
  updateNode: (nodeId: string, updates: Partial<WorkflowNode>) => void;
  removeNode: (nodeId: string) => void;
  saveWorkflow: () => Promise<void>;
}

export const useWorkflowStore = create<WorkflowStore>()((set, get) => ({
  currentWorkflow: null,
  nodes: [],
  edges: [],
  isDirty: false,
  
  setCurrentWorkflow: (workflow) => set({
    currentWorkflow: workflow,
    nodes: workflow?.nodes || [],
    edges: workflow?.edges || [],
    isDirty: false
  }),
  
  addNode: (node) => {
    const { nodes } = get();
    set({ 
      nodes: [...nodes, node], 
      isDirty: true 
    });
  },
  
  saveWorkflow: async () => {
    const { currentWorkflow, nodes, edges } = get();
    if (!currentWorkflow) return;
    
    try {
      await workflowApi.updateWorkflow(currentWorkflow.id, {
        ...currentWorkflow,
        nodes,
        edges
      });
      set({ isDirty: false });
    } catch (error) {
      // Handle error appropriately
      throw new Error(`Failed to save workflow: ${error.message}`);
    }
  }
}));
```

### 3. API Integration Patterns

**✅ ALWAYS Handle API Calls Properly:**
```typescript
// Type-safe API client
class WorkflowApiClient {
  private readonly baseUrl: string;
  private readonly httpClient: AxiosInstance;
  
  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.httpClient = axios.create({
      baseURL: baseUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    this.setupInterceptors();
  }
  
  async getWorkflow(id: string): Promise<WorkflowDefinition> {
    try {
      const response = await this.httpClient.get<WorkflowDefinition>(`/workflows/${id}`);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        throw new ApiError(
          error.response?.status || 500,
          error.response?.data?.message || 'Failed to fetch workflow'
        );
      }
      throw error;
    }
  }
  
  async executeWorkflow(id: string, inputData: unknown): Promise<WorkflowExecutionResult> {
    const response = await this.httpClient.post<WorkflowExecutionResult>(
      `/workflows/${id}/execute`,
      { inputData }
    );
    return response.data;
  }
  
  private setupInterceptors(): void {
    // Request interceptor for auth
    this.httpClient.interceptors.request.use((config) => {
      const token = getAuthToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
    
    // Response interceptor for error handling
    this.httpClient.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Handle authentication error
          redirectToLogin();
        }
        return Promise.reject(error);
      }
    );
  }
}
```

---

## 📊 Monitoring & Observability

### 1. Logging Standards

**✅ ALWAYS Implement Structured Logging:**
```csharp
public class WorkflowExecutionService
{
    private readonly ILogger<WorkflowExecutionService> _logger;
    private readonly IMetricsCollector _metrics;
    
    public async Task<WorkflowExecutionResult> ExecuteAsync(
        Guid workflowId, 
        object inputData,
        CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity("WorkflowExecution");
        activity?.SetTag("workflow.id", workflowId.ToString());
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting workflow execution {WorkflowId}", workflowId);
            
            var result = await ExecuteWorkflowInternalAsync(workflowId, inputData, cancellationToken);
            
            stopwatch.Stop();
            _metrics.RecordWorkflowExecution(workflowId, result.Status, stopwatch.Elapsed);
            
            _logger.LogInformation(
                "Completed workflow execution {WorkflowId} with status {Status} in {Duration}ms",
                workflowId, result.Status, stopwatch.ElapsedMilliseconds);
                
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordWorkflowExecutionError(workflowId, ex.GetType().Name);
            
            _logger.LogError(ex, 
                "Failed workflow execution {WorkflowId} after {Duration}ms", 
                workflowId, stopwatch.ElapsedMilliseconds);
                
            throw;
        }
    }
}
```

### 2. Metrics Collection

**✅ ALWAYS Add Relevant Metrics:**
```csharp
public class MetricsCollector : IMetricsCollector
{
    private readonly Counter _workflowExecutions = Metrics
        .CreateCounter("workflow_executions_total", "Total workflow executions", "status", "workflow_type");
        
    private readonly Histogram _executionDuration = Metrics
        .CreateHistogram("workflow_execution_duration_seconds", "Workflow execution duration");
        
    private readonly Gauge _activeExecutions = Metrics
        .CreateGauge("workflow_active_executions", "Currently active workflow executions");
    
    public void RecordWorkflowExecution(Guid workflowId, ExecutionStatus status, TimeSpan duration)
    {
        _workflowExecutions.WithLabels(status.ToString(), GetWorkflowType(workflowId)).Inc();
        _executionDuration.Observe(duration.TotalSeconds);
    }
    
    public void RecordActiveExecution(int count)
    {
        _activeExecutions.Set(count);
    }
}
```

---

## 🔒 Production Deployment Guardrails

### 1. Configuration Management

**✅ ALWAYS Use Secure Configuration:**
```csharp
// appsettings.Production.json structure
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "WorkflowPlatform": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "#{DATABASE_CONNECTION_STRING}#",
    "Redis": "#{REDIS_CONNECTION_STRING}#"
  },
  "Jwt": {
    "Secret": "#{JWT_SECRET}#",
    "Issuer": "#{JWT_ISSUER}#",
    "Audience": "#{JWT_AUDIENCE}#",
    "ExpirationHours": 24
  },
  "WorkflowEngine": {
    "MaxConcurrentExecutions": 1000,
    "DefaultTimeoutMinutes": 30,
    "RetryAttempts": 3
  }
}

// Environment variable validation
public static class ConfigurationValidator
{
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        var requiredSettings = new[]
        {
            "ConnectionStrings:DefaultConnection",
            "Jwt:Secret",
            "Jwt:Issuer",
            "Jwt:Audience"
        };
        
        var missingSettings = requiredSettings
            .Where(setting => string.IsNullOrEmpty(configuration[setting]))
            .ToList();
            
        if (missingSettings.Any())
        {
            throw new InvalidOperationException(
                $"Missing required configuration: {string.Join(", ", missingSettings)}");
        }
    }
}
```

### 2. Health Checks

**✅ ALWAYS Implement Comprehensive Health Checks:**
```csharp
public static class HealthCheckExtensions
{
    public static IServiceCollection AddWorkflowHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "postgresql",
                tags: new[] { "db", "ready" })
            .AddRedis(
                connectionString: configuration.GetConnectionString("Redis"),
                name: "redis",
                tags: new[] { "cache", "ready" })
            .AddRabbitMQ(
                connectionString: configuration.GetConnectionString("RabbitMQ"),
                name: "rabbitmq",
                tags: new[] { "messaging", "ready" })
            .AddCheck<WorkflowEngineHealthCheck>("workflow-engine", tags: new[] { "custom", "ready" });
            
        return services;
    }
}

public class WorkflowEngineHealthCheck : IHealthCheck
{
    private readonly IWorkflowEngine _engine;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _engine.IsHealthyAsync(cancellationToken);
            return isHealthy 
                ? HealthCheckResult.Healthy("Workflow engine is running")
                : HealthCheckResult.Unhealthy("Workflow engine is not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Workflow engine check failed", ex);
        }
    }
}
```

---

## ⚠️ Critical DON'Ts

### 1. Security Anti-Patterns

```csharp
// ❌ NEVER DO THESE:

// 1. Hard-coded secrets
var connectionString = "Server=prod;Database=workflow;User=sa;Password=admin123;";

// 2. SQL injection vulnerabilities
var sql = $"SELECT * FROM workflows WHERE name = '{userInput}'";

// 3. Unvalidated deserialization
var obj = JsonConvert.DeserializeObject(untrustedJson);

// 4. Information disclosure in errors
catch (Exception ex) 
{ 
    return BadRequest(ex.ToString()); 
}

// 5. Missing authorization
public async Task<IActionResult> DeleteWorkflow(Guid id)
{
    // No authorization check before deletion
}
```

### 2. Performance Anti-Patterns

```csharp
// ❌ NEVER DO THESE:

// 1. Blocking async calls
var result = SomeAsyncOperation().Result;

// 2. Missing ConfigureAwait in libraries
await SomeOperation(); // Should be: await SomeOperation().ConfigureAwait(false);

// 3. N+1 queries
foreach (var workflow in workflows)
{
    var executions = await GetExecutionsForWorkflow(workflow.Id);
}

// 4. Memory leaks with events
SomeEvent += Handler; // Missing unsubscription

// 5. Inappropriate async void
public async void HandleEvent() // Should return Task
```

### 3. Architecture Anti-Patterns

```csharp
// ❌ NEVER DO THESE:

// 1. Tight coupling between layers
public class WorkflowController
{
    private readonly WorkflowDbContext _context; // Should use repository/mediator
}

// 2. Static dependencies
public class WorkflowService
{
    public void Execute()
    {
        DatabaseHelper.ExecuteQuery(); // Static dependency
    }
}

// 3. God objects/methods
public class WorkflowProcessor
{
    public async Task ProcessEverything() // 500+ lines method
    {
        // Handles validation, execution, persistence, notifications, etc.
    }
}
```

---

## ✅ Quality Checklist

Before submitting any generated code, ensure:

### Code Quality
- [ ] Follows SOLID principles
- [ ] Has proper error handling
- [ ] Includes comprehensive logging
- [ ] Has appropriate unit tests
- [ ] Follows naming conventions
- [ ] No code smells or anti-patterns

### Security
- [ ] Input validation implemented
- [ ] Authorization checks in place
- [ ] No sensitive data in logs
- [ ] Secure configuration management
- [ ] No hard-coded secrets

### Performance
- [ ] Async/await used correctly
- [ ] Proper resource disposal
- [ ] Database queries optimized
- [ ] Caching implemented where appropriate
- [ ] Memory usage considered

### Architecture
- [ ] Clean Architecture layers respected
- [ ] Dependency injection used properly
- [ ] Strategy pattern implemented correctly
- [ ] Single Responsibility Principle followed
- [ ] Open/Closed Principle followed

### Testing
- [ ] Unit tests cover happy path
- [ ] Unit tests cover error scenarios
- [ ] Integration tests for critical paths
- [ ] Test names are descriptive
- [ ] Arrange-Act-Assert pattern followed

### Documentation
- [ ] XML documentation for public APIs
- [ ] Complex algorithms explained
- [ ] Configuration requirements documented
- [ ] Deployment considerations noted

---

## 🎓 Learning Resources

### Recommended Reading
- **Clean Architecture**: Robert C. Martin
- **Domain-Driven Design**: Eric Evans
- **Microservices Patterns**: Chris Richardson
- **C# in Depth**: Jon Skeet
- **React Patterns**: Michael Chan

### Internal Guidelines
- Review the project's coding standards document
- Follow the established Git workflow
- Use the project's code review checklist
- Participate in architecture decision records (ADRs)

---

## 🔧 IDE Integration & Configuration

### 1. Visual Studio / VS Code Settings

**EditorConfig (.editorconfig):**
```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.{js,ts,tsx,json}]
indent_size = 2

[*.yml]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

**Copilot Configuration (.vscode/settings.json):**
```json
{
  "github.copilot.enable": {
    "*": true,
    "yaml": true,
    "plaintext": false,
    "markdown": true,
    "csharp": true,
    "typescript": true,
    "javascript": true
  },
  "github.copilot.inlineSuggest.enable": true,
  "github.copilot.advanced": {
    "secret_key": "workflow-platform-2024",
    "length": 3000,
    "temperature": 0.1,
    "top_p": 1,
    "listCount": 10,
    "indentationMode": {
      "python": "space",
      "javascript": "space", 
      "typescript": "space",
      "csharp": "space"
    }
  }
}
```

### 2. Code Analysis Rules

**Directory.Build.props:**
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701;CS1591</WarningsNotAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.16.0.82469" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

---

## 🎯 Context-Aware Code Generation

### 1. Project Context Recognition

**When Copilot detects these patterns, apply specific rules:**

#### Backend (.NET) Context
```csharp
// File: *Controller.cs, *Service.cs, *Repository.cs
// Pattern Recognition: namespace WorkflowPlatform.*
// Apply: Enterprise patterns, security, async/await, logging
```

#### Frontend (React/TypeScript) Context
```typescript
// File: *.tsx, *.ts in src/
// Pattern Recognition: import React, Next.js patterns
// Apply: TypeScript strict mode, React best practices, accessibility
```

#### Infrastructure Context
```yaml
# File: *.yml, *.yaml, Dockerfile, docker-compose.*
# Pattern Recognition: Kubernetes, Docker, CI/CD
# Apply: Security hardening, resource limits, health checks
```

### 2. Smart Context Suggestions

**✅ When suggesting in Controller context:**
```csharp
// Copilot should ALWAYS suggest:
[HttpPost]
[Authorize]
[ProducesResponseType(typeof(WorkflowExecutionResult), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ExecuteWorkflow(
    [FromRoute] Guid workflowId,
    [FromBody] ExecuteWorkflowRequest request,
    CancellationToken cancellationToken = default)
{
    // Implementation with proper error handling
}
```

**✅ When suggesting in Strategy context:**
```csharp
// Copilot should ALWAYS include lifecycle methods:
public override async Task<NodeExecutionResult> PreprocessAsync(
    NodeExecutionContext context, 
    CancellationToken cancellationToken)
{
    // Validation logic
}

public override async Task<NodeExecutionResult> ExecuteAsync(
    NodeExecutionContext context, 
    CancellationToken cancellationToken)
{
    // Core execution logic
}

public override async Task<NodeExecutionResult> PostprocessAsync(
    NodeExecutionContext context, 
    NodeExecutionResult executionResult, 
    CancellationToken cancellationToken)
{
    // Output transformation
}

public override async Task FinalizationAsync(
    NodeExecutionContext context, 
    NodeExecutionResult executionResult, 
    CancellationToken cancellationToken)
{
    // Cleanup and persistence
}
```

---

## 🧩 Advanced Patterns & Templates

### 1. Repository Pattern Implementation

**✅ ALWAYS Generate This Pattern:**
```csharp
public interface IWorkflowRepository : IRepository<WorkflowAggregate, Guid>
{
    Task<WorkflowAggregate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowAggregate>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public class WorkflowRepository : Repository<WorkflowAggregate, Guid>, IWorkflowRepository
{
    public WorkflowRepository(WorkflowDbContext context, ILogger<WorkflowRepository> logger) 
        : base(context, logger) { }

    public async Task<WorkflowAggregate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .FirstOrDefaultAsync(w => w.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<WorkflowAggregate>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .AnyAsync(w => w.Id == id, cancellationToken);
    }
}
```

### 2. CQRS Command/Query Handlers

**✅ ALWAYS Generate This Pattern:**
```csharp
// Command
public record CreateWorkflowCommand(
    string Name,
    string Description,
    WorkflowDefinition Definition,
    Guid CreatedBy) : IRequest<CreateWorkflowResult>;

// Command Handler
public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, CreateWorkflowResult>
{
    private readonly IWorkflowRepository _repository;
    private readonly IValidator<CreateWorkflowCommand> _validator;
    private readonly ILogger<CreateWorkflowHandler> _logger;

    public CreateWorkflowHandler(
        IWorkflowRepository repository,
        IValidator<CreateWorkflowCommand> validator,
        ILogger<CreateWorkflowHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CreateWorkflowResult> Handle(
        CreateWorkflowCommand request, 
        CancellationToken cancellationToken)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return CreateWorkflowResult.Failure(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            // Check if workflow with same name exists
            var existingWorkflow = await _repository.GetByNameAsync(request.Name, cancellationToken);
            if (existingWorkflow != null)
            {
                return CreateWorkflowResult.Failure("Workflow with this name already exists");
            }

            // Create workflow aggregate
            var workflow = WorkflowAggregate.Create(
                request.Name, 
                request.Description, 
                request.CreatedBy);

            // Add definition
            workflow.UpdateDefinition(request.Definition);

            // Persist
            await _repository.AddAsync(workflow, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created workflow {WorkflowId} with name {WorkflowName}", 
                workflow.Id, workflow.Name);

            return CreateWorkflowResult.Success(workflow.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workflow {WorkflowName}", request.Name);
            throw;
        }
    }
}

// Query
public record GetWorkflowQuery(Guid WorkflowId) : IRequest<GetWorkflowResult>;

// Query Handler
public class GetWorkflowHandler : IRequestHandler<GetWorkflowQuery, GetWorkflowResult>
{
    private readonly IWorkflowRepository _repository;
    private readonly IMapper _mapper;

    public async Task<GetWorkflowResult> Handle(
        GetWorkflowQuery request, 
        CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.WorkflowId, cancellationToken);
        
        if (workflow == null)
        {
            return GetWorkflowResult.NotFound();
        }

        var dto = _mapper.Map<WorkflowDto>(workflow);
        return GetWorkflowResult.Success(dto);
    }
}
```

### 3. Event Sourcing Pattern

**✅ ALWAYS Generate This Pattern:**
```csharp
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

public record WorkflowExecutionStartedEvent(
    Guid WorkflowId,
    Guid ExecutionId,
    object InputData,
    Guid StartedBy) : DomainEvent;

public record WorkflowExecutionCompletedEvent(
    Guid WorkflowId,
    Guid ExecutionId,
    ExecutionStatus Status,
    object? OutputData,
    TimeSpan Duration) : DomainEvent;

// Aggregate Root with Event Sourcing
public abstract class AggregateRoot<TId> where TId : struct
{
    private readonly List<DomainEvent> _uncommittedEvents = new();

    public TId Id { get; protected set; }
    public int Version { get; protected set; }

    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected void Apply(DomainEvent domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
        Version++;
    }

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    public void LoadFromHistory(IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event, false);
            Version++;
        }
    }

    protected abstract void ApplyEvent(DomainEvent domainEvent, bool isNew = true);
}
```

### 4. Advanced Frontend Patterns

**✅ React Custom Hooks Pattern:**
```typescript
// Custom hook for workflow management
export const useWorkflow = (workflowId: string) => {
  const [state, setState] = useState<WorkflowState>({
    workflow: null,
    isLoading: true,
    error: null,
    isDirty: false
  });

  const { data: workflow, isLoading, error, mutate } = useSWR(
    `/api/workflows/${workflowId}`,
    workflowApi.getWorkflow,
    {
      refreshInterval: 0,
      revalidateOnFocus: false,
      onError: (error) => setState(prev => ({ ...prev, error }))
    }
  );

  const updateWorkflow = useCallback(async (updates: Partial<WorkflowDefinition>) => {
    if (!workflow) return;

    setState(prev => ({ ...prev, isDirty: true }));

    try {
      const updatedWorkflow = { ...workflow, ...updates };
      await mutate(workflowApi.updateWorkflow(workflowId, updatedWorkflow), {
        optimisticData: updatedWorkflow,
        rollbackOnError: true
      });
      setState(prev => ({ ...prev, isDirty: false }));
    } catch (error) {
      setState(prev => ({ ...prev, error: error as Error }));
    }
  }, [workflow, workflowId, mutate]);

  const executeWorkflow = useCallback(async (inputData: unknown) => {
    if (!workflow) return;

    try {
      return await workflowApi.executeWorkflow(workflowId, inputData);
    } catch (error) {
      setState(prev => ({ ...prev, error: error as Error }));
      throw error;
    }
  }, [workflow, workflowId]);

  return {
    workflow,
    isLoading,
    error,
    isDirty: state.isDirty,
    updateWorkflow,
    executeWorkflow,
    refetch: () => mutate()
  };
};

// Error boundary component
export class WorkflowErrorBoundary extends React.Component<
  { children: React.ReactNode; fallback?: React.ComponentType<{ error: Error }> },
  { hasError: boolean; error?: Error }
> {
  constructor(props: any) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Workflow error boundary caught an error:', error, errorInfo);
    
    // Report to monitoring service
    if (typeof window !== 'undefined' && window.gtag) {
      window.gtag('event', 'exception', {
        description: error.message,
        fatal: false
      });
    }
  }

  render() {
    if (this.state.hasError) {
      const FallbackComponent = this.props.fallback || DefaultErrorFallback;
      return <FallbackComponent error={this.state.error!} />;
    }

    return this.props.children;
  }
}
```

---

## 🔍 Code Review Integration

### 1. Pre-commit Hooks

**✅ Always suggest these hooks (.husky/pre-commit):**
```bash
#!/usr/bin/env sh
. "$(dirname -- "$0")/_/husky.sh"

# .NET Backend checks
echo "🔍 Running backend code analysis..."
dotnet format --verify-no-changes --verbosity minimal
dotnet build --configuration Release --no-restore
dotnet test --no-build --configuration Release --logger "console;verbosity=minimal"

# Frontend checks
echo "🔍 Running frontend code analysis..."
cd workflow-platform-frontend
npm run type-check
npm run lint
npm run test:ci

# Security scan
echo "🔍 Running security scan..."
npm audit --audit-level moderate
dotnet list package --vulnerable --include-transitive

echo "✅ All checks passed!"
```

### 2. Pull Request Templates

**✅ Always suggest this PR template (.github/pull_request_template.md):**
```markdown
## Summary
Brief description of the changes made.

## Related Tickets
- Closes WOP-XXX
- Related to WOP-XXX

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Refactoring (no functional changes)

## Testing
- [ ] Unit tests ad