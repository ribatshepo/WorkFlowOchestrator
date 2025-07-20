# Node Execution Engine Implementation

This document describes the complete implementation of the **Node Execution Engine** for the Workflow Orchestration Platform, following the Strategy pattern with a 4-phase lifecycle as specified in Epic WOP-E001.2.

## 🏗️ Architecture Overview

The Node Execution Engine implements a robust, scalable architecture using the Strategy pattern with comprehensive error handling, retry logic, and observability features.

### Core Components

```
NodeExecutionEngine
├── INodeExecutionStrategy (Strategy Interface)
├── BaseNodeExecutionStrategy (Abstract Base)
├── NodeStrategyFactory (Strategy Factory)
├── Concrete Strategies
│   ├── HttpRequestNodeStrategy
│   ├── DatabaseQueryNodeStrategy
│   └── EmailNotificationNodeStrategy
└── Supporting Infrastructure
    ├── RetryPolicy (Polly-based)
    ├── CircuitBreaker (Custom implementation)
    ├── MetricsCollector (Observability)
    └── ValidationResult (Input/Output validation)
```

## 🔄 4-Phase Execution Lifecycle

Every node execution follows this strict 4-phase lifecycle:

### 1. **Preprocessing Phase**
- **Purpose**: Input validation and execution context setup
- **Responsibilities**:
  - Validate node configuration
  - Validate input data
  - Setup execution resources
  - Perform security checks

### 2. **Execute Phase**
- **Purpose**: Core business logic execution
- **Responsibilities**:
  - Perform the main node operation
  - Handle node-specific logic
  - Apply retry policies and circuit breakers
  - Generate primary output

### 3. **Postprocessing Phase**
- **Purpose**: Output transformation and validation
- **Responsibilities**:
  - Transform output data formats
  - Validate output data
  - Apply business rules to results
  - Prepare data for next nodes

### 4. **Finalization Phase**
- **Purpose**: Cleanup and state persistence
- **Responsibilities**:
  - Clean up resources
  - Persist execution state
  - Trigger completion events
  - Update metrics and logging

## 📦 Key Features Implemented

### ✅ Strategy Pattern Implementation
- **INodeExecutionStrategy**: Core strategy interface
- **BaseNodeExecutionStrategy**: Common lifecycle management
- **NodeStrategyFactory**: Dynamic strategy resolution
- **Dependency Injection**: Full DI integration

### ✅ Node Types Implemented
1. **HttpRequestNodeStrategy**: REST API calls with authentication
2. **DatabaseQueryNodeStrategy**: SQL execution with multiple providers
3. **EmailNotificationNodeStrategy**: SMTP email sending with templates

### ✅ Resilience Features
- **Retry Policy**: Exponential backoff with jitter
- **Circuit Breaker**: Custom implementation with state tracking
- **Timeout Handling**: Configurable timeouts per node type
- **Cancellation Support**: Full CancellationToken support

### ✅ Observability & Monitoring
- **Metrics Collection**: Performance and error metrics
- **Structured Logging**: Comprehensive logging throughout
- **Lifecycle Tracking**: Duration tracking per phase
- **Error Classification**: Detailed error categorization

### ✅ Validation Framework
- **Input Validation**: FluentValidation-based configuration validation
- **Output Validation**: Result validation after execution
- **Security Validation**: SQL injection and dangerous operation detection
- **Schema Validation**: Type-safe configuration handling

## 🚀 Usage Examples

### Basic Node Execution

```csharp
// Create execution context
var context = new NodeExecutionContext(
    nodeId: Guid.NewGuid(),
    nodeType: "HttpRequest",
    workflowId: Guid.NewGuid(),
    executionId: Guid.NewGuid(),
    inputData: null,
    configuration: new Dictionary<string, object>
    {
        { "HttpConfig", new HttpRequestNodeConfiguration
        {
            Url = "https://api.example.com/data",
            Method = "GET",
            Timeout = TimeSpan.FromSeconds(30)
        }}
    });

// Execute through all 4 phases
var result = await nodeExecutionEngine.ExecuteNodeAsync(context);

if (result.IsSuccess)
{
    Console.WriteLine($"Success: {result.OutputData}");
}
else
{
    Console.WriteLine($"Failed: {result.ErrorMessage}");
}
```

### Execution with Retry Logic

```csharp
// Execute with automatic retry on transient failures
var result = await nodeExecutionEngine.ExecuteNodeWithRetryAsync(
    context,
    maxRetries: 3,
    retryDelay: TimeSpan.FromSeconds(1));
```

### HTTP Request Node Example

```csharp
var httpConfig = new HttpRequestNodeConfiguration
{
    Url = "https://api.example.com/users",
    Method = "POST",
    Headers = new Dictionary<string, string>
    {
        { "Authorization", "Bearer token123" },
        { "Content-Type", "application/json" }
    },
    Body = """{"name": "John Doe", "email": "john@example.com"}""",
    Timeout = TimeSpan.FromSeconds(30),
    Authentication = new Dictionary<string, object>
    {
        { "Type", "Bearer" },
        { "Token", "your-jwt-token" }
    }
};
```

### Database Query Node Example

```csharp
var dbConfig = new DatabaseQueryNodeConfiguration
{
    ConnectionString = "Host=localhost;Database=workflow;Username=user;Password=pass",
    Query = "SELECT * FROM users WHERE active = @active ORDER BY created_date DESC",
    Parameters = new Dictionary<string, object>
    {
        { "@active", true }
    },
    Timeout = TimeSpan.FromSeconds(30),
    IsReadOnly = true,
    DatabaseProvider = "PostgreSQL"
};
```

### Email Notification Node Example

```csharp
var emailConfig = new EmailNotificationNodeConfiguration
{
    SmtpServer = "smtp.gmail.com",
    SmtpPort = 587,
    EnableSsl = true,
    Username = "sender@example.com",
    Password = "app-password",
    FromAddress = "sender@example.com",
    FromDisplayName = "Workflow System",
    ToAddresses = new List<string> { "recipient@example.com" },
    Subject = "Workflow Notification",
    Body = "Your workflow has completed successfully.",
    IsBodyHtml = false
};
```

## 🛠️ Configuration & Setup

### Dependency Injection Registration

```csharp
services.AddApplication(); // Registers all node execution services

// Or register individually:
services.AddNodeExecutionEngine();
```

### Required NuGet Packages

```xml
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="Npgsql" Version="8.0.1" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.2" />
<PackageReference Include="MySqlConnector" Version="2.3.5" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
```

## 🧪 Testing

### Unit Test Coverage

- **BaseNodeExecutionStrategy**: Lifecycle management tests
- **HttpRequestNodeStrategy**: HTTP operation tests with mocking
- **NodeExecutionEngine**: Full execution pipeline tests
- **RetryPolicy**: Exponential backoff and retry logic tests
- **CircuitBreaker**: State transitions and failure handling tests
- **Validators**: Configuration validation tests

### Test Example

```csharp
[Fact]
public async Task ExecuteAsync_WithValidRequest_Should_ReturnSuccess()
{
    // Arrange
    var context = CreateValidExecutionContext();
    SetupHttpClientMock(HttpStatusCode.OK, "{ \"result\": \"success\" }");

    // Act
    var result = await strategy.ExecuteAsync(context, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.OutputData.Should().NotBeNull();
}
```

## 📊 Metrics & Monitoring

### Collected Metrics

- **Node Execution Duration**: Total time per node execution
- **Phase Duration**: Time spent in each lifecycle phase
- **Retry Attempts**: Number of retry attempts per node type
- **Circuit Breaker State**: State changes and failure counts
- **Error Classification**: Error types and frequencies

### Example Metrics Output

```
Node execution recorded - NodeType: HttpRequest, Status: Completed, Duration: 245ms
Lifecycle phase duration - NodeType: HttpRequest, Phase: Preprocessing, Duration: 5ms
Lifecycle phase duration - NodeType: HttpRequest, Phase: Execute, Duration: 230ms
Lifecycle phase duration - NodeType: HttpRequest, Phase: Postprocessing, Duration: 3ms
Lifecycle phase duration - NodeType: HttpRequest, Phase: Finalization, Duration: 7ms
```

## 🔒 Security Features

### Input Validation
- URL validation for HTTP requests
- SQL injection prevention for database queries
- Email address validation
- Configuration schema validation

### Authentication Support
- Bearer token authentication
- Basic authentication
- API key authentication
- Custom header authentication

### Security Best Practices
- Sensitive data logging prevention
- Secure connection string handling
- Timeout enforcement
- Resource disposal

## 🚀 Performance Features

### Optimizations
- **Async/Await**: Full async implementation throughout
- **Resource Management**: Proper disposal with using statements
- **Connection Pooling**: Database connection reuse
- **HTTP Client Reuse**: Shared HTTP client instances
- **Memory Efficiency**: Streaming for large responses

### Scalability
- **Cancellation Support**: Graceful cancellation handling
- **Timeout Management**: Configurable timeouts per operation
- **Circuit Breakers**: Prevent cascade failures
- **Retry Logic**: Exponential backoff with jitter

## 📝 Implementation Status

### ✅ Completed Features

| Feature | Status | Implementation |
|---------|--------|----------------|
| **WOP-015**: Strategy Pattern | ✅ Complete | `INodeExecutionStrategy`, `NodeStrategyFactory` |
| **WOP-016**: Base Strategy | ✅ Complete | `BaseNodeExecutionStrategy` |
| **WOP-017**: Preprocessing | ✅ Complete | Input validation and context setup |
| **WOP-018**: Execute Phase | ✅ Complete | Core business logic execution |
| **WOP-019**: Postprocessing | ✅ Complete | Output transformation and validation |
| **WOP-020**: Finalization | ✅ Complete | Resource cleanup and state persistence |
| **WOP-021**: HTTP Request Node | ✅ Complete | `HttpRequestNodeStrategy` |
| **WOP-022**: Database Query Node | ✅ Complete | `DatabaseQueryNodeStrategy` |
| **WOP-023**: Email Notification Node | ✅ Complete | `EmailNotificationNodeStrategy` |
| **WOP-024**: Validation Framework | ✅ Complete | FluentValidation-based validators |
| **WOP-025**: Retry Logic | ✅ Complete | Polly-based retry with circuit breakers |

### 🎯 Key Achievements

1. **Production-Ready**: Enterprise-grade error handling and logging
2. **Highly Testable**: Comprehensive unit test coverage
3. **Extensible**: Easy to add new node types
4. **Observable**: Rich metrics and logging
5. **Resilient**: Robust retry and circuit breaker implementation
6. **Secure**: Input validation and security best practices

## 🔧 Extending the Engine

### Adding a New Node Type

1. **Create Configuration Class**:
```csharp
public class CustomNodeConfiguration
{
    public string CustomProperty { get; set; } = string.Empty;
    // Add configuration properties
}
```

2. **Implement Strategy**:
```csharp
public class CustomNodeStrategy : BaseNodeExecutionStrategy
{
    public override string NodeType => "CustomNode";
    
    public override async Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        // Implement custom logic
        return NodeExecutionResult.Success("Custom result");
    }
    
    // Implement other abstract methods...
}
```

3. **Register in DI**:
```csharp
services.AddScoped<INodeExecutionStrategy, CustomNodeStrategy>();
```

This completes the comprehensive implementation of the Node Execution Engine for Epic WOP-E001.2, providing a robust, scalable, and production-ready foundation for workflow node execution.
