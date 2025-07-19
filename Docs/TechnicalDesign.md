# Workflow Orchestration Platform - Technical Design Document

This technical design document provides a complete implementation roadmap for the Workflow Orchestration Platform, following Clean Architecture principles with Strategy pattern implementation and the required node execution lifecycle. All duplications have been removed while maintaining comprehensive coverage of the implementation requirements.# Workflow Orchestration Platform - Technical Design Document

## Document Information

| Field | Value |
|-------|-------|
| **Document Version** | 1.0 |
| **Date** | December 2024 |
| **Author** | Development Team |
| **Status** | Draft |

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Architecture](#2-system-architecture)
3. [Backend Implementation (.NET 8)](#3-backend-implementation-net-8)
4. [Frontend Implementation (Next.js 14)](#4-frontend-implementation-nextjs-14)
5. [Database Design](#5-database-design)
6. [Infrastructure Setup](#6-infrastructure-setup)
7. [Security Implementation](#7-security-implementation)
8. [Testing Strategy](#8-testing-strategy)
9. [Deployment Configuration](#9-deployment-configuration)
10. [Development Setup Guide](#10-development-setup-guide)

---

## 1. Executive Summary

This technical design document outlines the implementation for the Workflow Orchestration Platform based on Clean Architecture principles with N-Tier separation, Strategy pattern implementation, and the preprocessing → execute → postprocessing → finalization node execution lifecycle.

### 1.1 Technology Stack

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Backend Framework** | .NET | 8.0 | Web API, gRPC Services |
| **Frontend Framework** | Next.js | 14+ | React-based UI |
| **Database** | PostgreSQL | 15+ | Primary data store |
| **Cache** | Redis | 7+ | Caching and session storage |
| **Message Queue** | RabbitMQ | 3.12+ | Asynchronous processing |
| **Monitoring** | Prometheus | 2.45+ | Metrics collection |
| **Visualization** | Grafana | 10.0+ | Dashboards |

---

## 2. System Architecture

### 2.1 Clean Architecture Layers

```mermaid
graph TB
    subgraph "Presentation Layer"
        A[Next.js UI] 
        B[REST API]
        C[gRPC Services]
        D[SignalR Hubs]
    end
    
    subgraph "Application Layer"
        E[Command/Query Handlers]
        F[Node Strategies]
    end
    
    subgraph "Domain Layer"
        G[Workflow Aggregate]
        H[Business Logic]
    end
    
    subgraph "Infrastructure Layer"
        I[PostgreSQL]
        J[Redis Cache]
        K[RabbitMQ]
    end
    
    A --> B
    B --> E
    C --> E
    D --> E
    E --> G
    G --> I
    F --> J
    E --> K
```

### 2.2 Node Execution Lifecycle

```mermaid
sequenceDiagram
    participant E as Execution Engine
    participant S as Node Strategy
    participant D as Database
    
    E->>S: Preprocessing
    S->>D: Validate & Setup
    S-->>E: Ready
    
    E->>S: Execute
    S-->>E: Result
    
    E->>S: Postprocessing
    S-->>E: Transformed Result
    
    E->>S: Finalization
    S->>D: Persist State
    S-->>E: Complete
```

---

## 3. Backend Implementation (.NET 8)

### 3.1 Project Setup

```bash
# Create solution and projects
dotnet new sln -n WorkflowPlatform

# Create projects following Clean Architecture
dotnet new classlib -n WorkflowPlatform.Domain
dotnet new classlib -n WorkflowPlatform.Application  
dotnet new classlib -n WorkflowPlatform.Infrastructure
dotnet new webapi -n WorkflowPlatform.Api
dotnet new classlib -n WorkflowPlatform.Contracts
dotnet new xunit -n WorkflowPlatform.Tests.Unit
dotnet new xunit -n WorkflowPlatform.Tests.Integration

# Add projects to solution
dotnet sln add WorkflowPlatform.Domain/WorkflowPlatform.Domain.csproj
dotnet sln add WorkflowPlatform.Application/WorkflowPlatform.Application.csproj
dotnet sln add WorkflowPlatform.Infrastructure/WorkflowPlatform.Infrastructure.csproj
dotnet sln add WorkflowPlatform.Api/WorkflowPlatform.Api.csproj
dotnet sln add WorkflowPlatform.Contracts/WorkflowPlatform.Contracts.csproj
dotnet sln add WorkflowPlatform.Tests.Unit/WorkflowPlatform.Tests.Unit.csproj
dotnet sln add WorkflowPlatform.Tests.Integration/WorkflowPlatform.Tests.Integration.csproj

# Add project references
dotnet add WorkflowPlatform.Application reference WorkflowPlatform.Domain
dotnet add WorkflowPlatform.Infrastructure reference WorkflowPlatform.Application
dotnet add WorkflowPlatform.Api reference WorkflowPlatform.Infrastructure
dotnet add WorkflowPlatform.Api reference WorkflowPlatform.Contracts
```

### 3.2 Package Installation

#### Domain Layer

```bash
cd WorkflowPlatform.Domain
dotnet add package MediatR --version 12.2.0
dotnet add package FluentValidation --version 11.8.1
```

#### Application Layer

```bash
cd ../WorkflowPlatform.Application
dotnet add package MediatR --version 12.2.0
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection --version 11.1.0
dotnet add package FluentValidation --version 11.8.1
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.8.1
dotnet add package AutoMapper --version 12.0.1
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
```

#### Infrastructure Layer

```bash
cd ../WorkflowPlatform.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package StackExchange.Redis --version 2.7.10
dotnet add package MassTransit --version 8.1.3
dotnet add package MassTransit.RabbitMQ --version 8.1.3
dotnet add package Serilog --version 3.1.1
dotnet add package Serilog.Extensions.Hosting --version 8.0.0
dotnet add package prometheus-net --version 8.2.1
dotnet add package Polly --version 8.2.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
```

#### API Layer

```bash
cd ../WorkflowPlatform.Api
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
dotnet add package Microsoft.AspNetCore.SignalR --version 8.0.0
dotnet add package Grpc.AspNetCore --version 2.60.0
dotnet add package Grpc.AspNetCore.Web --version 2.60.0
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks --version 8.0.0
dotnet add package AspNetCore.HealthChecks.Redis --version 8.0.1
dotnet add package AspNetCore.HealthChecks.RabbitMQ --version 8.0.2
dotnet add package AspNetCore.HealthChecks.Npgsql --version 8.0.2
```

### 3.3 Strategy Pattern Implementation

```csharp
// Domain Interface
public interface INodeExecutionStrategy
{
    string NodeType { get; }
    Task<NodeExecutionResult> PreprocessAsync(NodeExecutionContext context, CancellationToken cancellationToken);
    Task<NodeExecutionResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken);
    Task<NodeExecutionResult> PostprocessAsync(NodeExecutionContext context, NodeExecutionResult executionResult, CancellationToken cancellationToken);
    Task FinalizationAsync(NodeExecutionContext context, NodeExecutionResult executionResult, CancellationToken cancellationToken);
}

// Base Strategy
public abstract class BaseNodeExecutionStrategy : INodeExecutionStrategy
{
    protected readonly ILogger _logger;
    protected readonly IMetricsCollector _metrics;

    public abstract string NodeType { get; }

    public virtual async Task<NodeExecutionResult> PreprocessAsync(
        NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateInputsAsync(context, cancellationToken);
        if (!validationResult.IsValid)
            return NodeExecutionResult.Failed(string.Join(", ", validationResult.Errors));

        await SetupExecutionContextAsync(context, cancellationToken);
        return NodeExecutionResult.Success();
    }

    public abstract Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context, CancellationToken cancellationToken);

    public virtual async Task<NodeExecutionResult> PostprocessAsync(
        NodeExecutionContext context, NodeExecutionResult executionResult, CancellationToken cancellationToken)
    {
        if (!executionResult.IsSuccess) return executionResult;
        
        var transformedResult = await TransformOutputAsync(context, executionResult, cancellationToken);
        return await ValidateOutputAsync(context, transformedResult, cancellationToken);
    }

    public virtual async Task FinalizationAsync(
        NodeExecutionContext context, NodeExecutionResult executionResult, CancellationToken cancellationToken)
    {
        await CleanupResourcesAsync(context, cancellationToken);
        await PersistExecutionStateAsync(context, executionResult, cancellationToken);
        await TriggerCompletionEventsAsync(context, executionResult, cancellationToken);
    }

    protected abstract Task<ValidationResult> ValidateInputsAsync(NodeExecutionContext context, CancellationToken cancellationToken);
    protected abstract Task SetupExecutionContextAsync(NodeExecutionContext context, CancellationToken cancellationToken);
    protected abstract Task<NodeExecutionResult> TransformOutputAsync(NodeExecutionContext context, NodeExecutionResult result, CancellationToken cancellationToken);
    protected abstract Task<NodeExecutionResult> ValidateOutputAsync(NodeExecutionContext context, NodeExecutionResult result, CancellationToken cancellationToken);
    protected abstract Task CleanupResourcesAsync(NodeExecutionContext context, CancellationToken cancellationToken);
    protected abstract Task PersistExecutionStateAsync(NodeExecutionContext context, NodeExecutionResult result, CancellationToken cancellationToken);
    protected abstract Task TriggerCompletionEventsAsync(NodeExecutionContext context, NodeExecutionResult result, CancellationToken cancellationToken);
}
```

### 3.4 Environment Configuration

```bash
# Initialize user secrets
cd WorkflowPlatform.Api
dotnet user-secrets init
dotnet user-secrets set "JWT_SECRET" "your-super-secret-jwt-key-that-is-at-least-256-bits-long"
dotnet user-secrets set "DB_PASSWORD" "your-secure-database-password"
dotnet user-secrets set "REDIS_PASSWORD" "your-secure-redis-password"
dotnet user-secrets set "RABBITMQ_PASSWORD" "your-secure-rabbitmq-password"
```

Create `.env.development`:

```bash
DATABASE_CONNECTION_STRING=Host=localhost;Database=workflow_dev;Username=workflow_user;Password=${DB_PASSWORD}
REDIS_CONNECTION_STRING=localhost:6379
RABBITMQ_CONNECTION_STRING=amqp://workflow_user:${RABBITMQ_PASSWORD}@localhost:5672/
JWT_ISSUER=WorkflowPlatform
JWT_AUDIENCE=WorkflowPlatform.Api
ASPNETCORE_ENVIRONMENT=Development
CORS_ORIGINS=http://localhost:3000,https://localhost:3000
LOG_LEVEL=Information
```

---

## 4. Frontend Implementation (Next.js 14)

### 4.1 Project Setup

```bash
# Create Next.js project
npx create-next-app@latest workflow-platform-frontend \
  --typescript --tailwind --eslint --app --src-dir --import-alias "@/*"

cd workflow-platform-frontend
```

### 4.2 Package Installation

```bash
# UI Components and Styling
npm install @radix-ui/react-slot@latest @radix-ui/react-dialog@latest @radix-ui/react-dropdown-menu@latest
npm install class-variance-authority@latest clsx@latest tailwind-merge@latest lucide-react@latest

# Workflow Designer
npm install reactflow@latest @xyflow/react@latest

# State Management
npm install zustand@latest @tanstack/react-query@latest

# Forms and Validation
npm install react-hook-form@latest @hookform/resolvers@latest zod@latest

# Data Visualization
npm install recharts@latest d3@latest @types/d3@latest

# Real-time Communication
npm install @microsoft/signalr@latest

# gRPC Web Client
npm install grpc-web@latest @types/google-protobuf@latest google-protobuf@latest

# HTTP Client and Utilities
npm install axios@latest date-fns@latest lodash@latest @types/lodash@latest uuid@latest @types/uuid@latest

# Development Dependencies
npm install --save-dev @testing-library/react@latest jest@latest jest-environment-jsdom@latest
npm install --save-dev prettier@latest eslint-config-prettier@latest
```

### 4.3 Project Structure

```bash
workflow-platform-frontend/
├── src/
│   ├── app/
│   │   ├── (auth)/login/page.tsx
│   │   ├── dashboard/
│   │   │   ├── workflows/[id]/edit/page.tsx
│   │   │   └── executions/[id]/page.tsx
│   │   └── layout.tsx
│   ├── components/
│   │   ├── ui/ (button, input, dialog, etc.)
│   │   ├── workflow/ (WorkflowDesigner, NodePalette, etc.)
│   │   └── execution/ (ExecutionDashboard, LogViewer, etc.)
│   ├── lib/
│   │   ├── api/ (client, workflow-client, etc.)
│   │   ├── stores/ (workflow-store, auth-store, etc.)
│   │   ├── hooks/ (useWorkflow, useAuth, etc.)
│   │   └── types/ (workflow, execution, node types)
│   └── proto/ (gRPC generated files)
```

### 4.4 Configuration Files

#### Environment Configuration

```bash
# .env.local
NEXT_PUBLIC_API_BASE_URL=http://localhost:5001
NEXT_PUBLIC_GRPC_BASE_URL=http://localhost:5001
NEXT_PUBLIC_SIGNALR_BASE_URL=http://localhost:5001
NEXT_PUBLIC_APP_NAME=Workflow Platform
NEXT_PUBLIC_ENVIRONMENT=development
```

#### Next.js Configuration

```javascript
// next.config.js
const nextConfig = {
  experimental: { appDir: true },
  typescript: { ignoreBuildErrors: process.env.NODE_ENV === 'production' },
  images: {
    domains: ['localhost'],
    remotePatterns: [{ protocol: 'http', hostname: 'localhost', port: '5001', pathname: '/api/files/**' }]
  },
  webpack: (config, { isServer }) => {
    if (!isServer) {
      config.resolve.fallback = { ...config.resolve.fallback, fs: false, net: false, tls: false };
    }
    return config;
  }
};
module.exports = nextConfig;
```

### 4.5 Core Implementation

#### Workflow Store (Zustand)

```typescript
// src/lib/stores/workflow-store.ts
interface WorkflowState {
  currentWorkflow: WorkflowDefinition | null;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  selectedNodes: string[];
  isDirty: boolean;
  
  setCurrentWorkflow: (workflow: WorkflowDefinition | null) => void;
  addNode: (node: WorkflowNode) => void;
  removeNode: (nodeId: string) => void;
  updateNodeData: (nodeId: string, data: Partial<WorkflowNode['data']>) => void;
  saveWorkflow: () => Promise<void>;
}

export const useWorkflowStore = create<WorkflowState>()((set, get) => ({
  currentWorkflow: null,
  nodes: [],
  edges: [],
  selectedNodes: [],
  isDirty: false,
  
  setCurrentWorkflow: (workflow) => set({
    currentWorkflow: workflow,
    nodes: workflow?.nodes || [],
    edges: workflow?.edges || [],
    isDirty: false
  }),
  
  addNode: (node) => {
    const { nodes } = get();
    set({ nodes: [...nodes, node], isDirty: true });
  },
  
  removeNode: (nodeId) => {
    const { nodes, edges } = get();
    set({
      nodes: nodes.filter(n => n.id !== nodeId),
      edges: edges.filter(e => e.source !== nodeId && e.target !== nodeId),
      isDirty: true
    });
  },
  
  updateNodeData: (nodeId, data) => {
    const { nodes } = get();
    set({
      nodes: nodes.map(node => 
        node.id === nodeId ? { ...node, data: { ...node.data, ...data } } : node
      ),
      isDirty: true
    });
  },
  
  saveWorkflow: async () => {
    // Implementation for saving workflow
  }
}));
```

#### Multi-Protocol API Client

```typescript
// src/lib/api/client.ts
export class ApiClient {
  private httpClient: AxiosInstance;
  private signalRConnections: Map<string, HubConnection> = new Map();

  constructor(baseURL: string) {
    this.httpClient = axios.create({ baseURL, timeout: 30000 });
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.httpClient.get<T>(url, config);
    return response.data;
  }

  async getSignalRConnection(hubName: string): Promise<HubConnection> {
    if (this.signalRConnections.has(hubName)) {
      return this.signalRConnections.get(hubName)!;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${this.httpClient.defaults.baseURL}/hubs/${hubName}`)
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    await connection.start();
    this.signalRConnections.set(hubName, connection);
    return connection;
  }
}
```

---

## 5. Database Design

### 5.1 PostgreSQL Schema

```sql
-- Create schema and tables
CREATE SCHEMA IF NOT EXISTS workflow;

CREATE TABLE workflow.workflow_definitions (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(2000),
    version INTEGER NOT NULL DEFAULT 1,
    definition_json JSONB NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL
);

CREATE TABLE workflow.workflow_instances (
    id UUID PRIMARY KEY,
    workflow_definition_id UUID NOT NULL REFERENCES workflow.workflow_definitions(id),
    status VARCHAR(50) NOT NULL,
    input_data JSONB,
    output_data JSONB,
    state_data JSONB,
    started_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    created_by UUID NOT NULL
);

CREATE TABLE workflow.node_executions (
    id UUID PRIMARY KEY,
    workflow_instance_id UUID NOT NULL REFERENCES workflow.workflow_instances(id) ON DELETE CASCADE,
    node_id VARCHAR(255) NOT NULL,
    node_type VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    input_data JSONB,
    output_data JSONB,
    error_message VARCHAR(4000),
    started_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    duration_ms INTEGER,
    retry_count INTEGER DEFAULT 0
);

-- Performance indexes
CREATE INDEX idx_workflow_definitions_name ON workflow.workflow_definitions(name);
CREATE INDEX idx_workflow_instances_status ON workflow.workflow_instances(status);
CREATE INDEX idx_node_executions_instance ON workflow.node_executions(workflow_instance_id);
```

### 5.2 Entity Framework Migration Commands

```bash
# Navigate to Infrastructure project
cd WorkflowPlatform.Infrastructure

# Add initial migration
dotnet ef migrations add InitialCreate --context WorkflowDbContext --output-dir Persistence/Migrations --startup-project ../WorkflowPlatform.Api

# Create database and apply migrations
dotnet ef database update --context WorkflowDbContext --startup-project ../WorkflowPlatform.Api

# Generate SQL script for production
dotnet ef migrations script --context WorkflowDbContext --startup-project ../WorkflowPlatform.Api --output migration.sql
```

---

## 6. Infrastructure Setup

### 6.1 Docker Configuration

#### Backend Dockerfile

```dockerfile
# WorkflowPlatform.Api/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkflowPlatform.Api/WorkflowPlatform.Api.csproj", "WorkflowPlatform.Api/"]
COPY ["WorkflowPlatform.Infrastructure/WorkflowPlatform.Infrastructure.csproj", "WorkflowPlatform.Infrastructure/"]
COPY ["WorkflowPlatform.Application/WorkflowPlatform.Application.csproj", "WorkflowPlatform.Application/"]
COPY ["WorkflowPlatform.Domain/WorkflowPlatform.Domain.csproj", "WorkflowPlatform.Domain/"]

RUN dotnet restore "WorkflowPlatform.Api/WorkflowPlatform.Api.csproj"
COPY . .
WORKDIR "/src/WorkflowPlatform.Api"
RUN dotnet publish "WorkflowPlatform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkflowPlatform.Api.dll"]
```

#### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: workflow_platform
      POSTGRES_USER: workflow_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports: ["5432:5432"]
    volumes: [postgres_data:/var/lib/postgresql/data]

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD}
    ports: ["6379:6379"]

  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    environment:
      RABBITMQ_DEFAULT_USER: workflow_user
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    ports: ["5672:5672", "15672:15672"]

  prometheus:
    image: prom/prometheus:v2.45.0
    ports: ["9090:9090"]
    volumes: ["./infrastructure/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml"]

volumes:
  postgres_data:
```

### 6.2 Kubernetes Configuration

#### Deployment

```yaml
# k8s/api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: workflow-api
  namespace: workflow-platform
spec:
  replicas: 3
  selector:
    matchLabels:
      app: workflow-api
  template:
    spec:
      containers:
      - name: workflow-api
        image: workflow-platform/api:latest
        ports: [{ containerPort: 80 }]
        env:
        - name: DATABASE_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: workflow-secrets
              key: database-connection-string
        livenessProbe:
          httpGet: { path: /health, port: 80 }
          initialDelaySeconds: 30
        readinessProbe:
          httpGet: { path: /health/ready, port: 80 }
          initialDelaySeconds: 5
        resources:
          requests: { memory: "256Mi", cpu: "250m" }
          limits: { memory: "512Mi", cpu: "500m" }
```

---

## 7. Security Implementation

### 7.1 JWT Authentication

```csharp
// JWT Configuration
public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
    var key = Encoding.ASCII.GetBytes(secretKey);

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    return services;
}
```

### 7.2 Password Security

```csharp
// Password Service
public class PasswordService : IPasswordService
{
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        var combined = new byte[48];
        Array.Copy(salt, 0, combined, 0, 16);
        Array.Copy(hash, 0, combined, 16, 32);

        return Convert.ToBase64String(combined);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var combined = Convert.FromBase64String(hash);
        var salt = new byte[16];
        var storedHash = new byte[32];
        Array.Copy(combined, 0, salt, 0, 16);
        Array.Copy(combined, 16, storedHash, 0, 32);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
```

---

## 8. Testing Strategy

### 8.1 Unit Testing

```csharp
// Test Base Class
public abstract class TestBase
{
    protected readonly ITestOutputHelper Output;
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddXUnit(Output));
        services.AddAutoMapper(typeof(WorkflowMappingProfile));
    }

    protected Mock<T> CreateMock<T>() where T : class => new Mock<T>();
}

// Domain Tests Example
public class WorkflowAggregateTests : TestBase
{
    [Fact]
    public void CreateWorkflow_WithValidData_ShouldCreateWorkflow()
    {
        // Arrange
        var name = "Test Workflow";
        var createdBy = Guid.NewGuid();

        // Act
        var workflow = WorkflowAggregate.Create(name, "Description", createdBy);

        // Assert
        workflow.Should().NotBeNull();
        workflow.Name.Should().Be(name);
        workflow.CreatedBy.Should().Be(createdBy);
    }
}
```

### 8.2 Integration Testing

```csharp
// Web Application Factory
public class WorkflowWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("workflow_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add test database
            services.AddDbContext<WorkflowDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();
    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();
}
```

### 8.3 Frontend Testing

```typescript
// Component Tests
describe('WorkflowDesigner', () => {
  it('renders workflow designer canvas', () => {
    render(<WorkflowDesigner workflowId="test-id" />);
    expect(screen.getByTestId('react-flow')).toBeInTheDocument();
  });

  it('adds node when dragged from palette', async () => {
    const user = userEvent.setup();
    render(<WorkflowDesigner workflowId="test-id" />);
    
    const httpNode = screen.getByText('HTTP Request');
    await user.click(httpNode);
    
    await waitFor(() => {
      expect(mockStore.addNode).toHaveBeenCalled();
    });
  });
});
```

---

## 9. Deployment Configuration

### 9.1 CI/CD Pipeline

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline
on:
  push:
    branches: [main, develop]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: test_password
          POSTGRES_USER: test_user
          POSTGRES_DB: workflow_test
        ports: [5432:5432]
    
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet
- uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    - name: Test
      run: dotnet test --configuration Release --collect:"XPlat Code Coverage"
      env:
        DATABASE_CONNECTION_STRING: "Host=localhost;Database=workflow_test;Username=test_user;Password=test_password"

  test-frontend:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: workflow-platform-frontend/package-lock.json
    - name: Test
      working-directory: workflow-platform-frontend
      run: |
        npm ci
        npm run test:coverage

  build-and-push:
    needs: [test-backend, test-frontend]
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Build and push API image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./WorkflowPlatform.Api/Dockerfile
        push: true
        tags: ghcr.io/${{ github.repository }}/api:${{ github.sha }}

  deploy-production:
    if: github.ref == 'refs/heads/main'
    needs: [build-and-push]
    runs-on: ubuntu-latest
    steps:
    - name: Deploy to production
      run: |
        helm upgrade --install workflow-platform ./helm/workflow-platform \
          --namespace workflow-platform \
          --set image.tag=${{ github.sha }} \
          --wait --timeout=15m
```

### 9.2 Helm Chart

```yaml
# helm/workflow-platform/values.yaml
api:
  replicaCount: 3
  image:
    repository: ghcr.io/company/workflow-platform/api
    tag: "latest"
  service:
    type: ClusterIP
    port: 80
  resources:
    requests: { memory: "256Mi", cpu: "250m" }
    limits: { memory: "512Mi", cpu: "500m" }
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10
    targetCPUUtilizationPercentage: 70

frontend:
  replicaCount: 2
  image:
    repository: ghcr.io/company/workflow-platform/frontend
    tag: "latest"
  service:
    type: ClusterIP
    port: 3000

ingress:
  enabled: true
  className: "nginx"
  hosts:
    - host: workflow.company.com
      paths:
        - path: /api
          backend: { service: { name: workflow-api-service, port: 80 } }
        - path: /
          backend: { service: { name: workflow-frontend-service, port: 3000 } }

postgresql:
  enabled: true
  auth:
    username: "workflow_user"
    database: "workflow_platform"
  primary:
    persistence: { enabled: true, size: 100Gi }

redis:
  enabled: true
  auth: { enabled: true }
  master:
    persistence: { enabled: true, size: 10Gi }

rabbitmq:
  enabled: true
  auth:
    username: "workflow_user"
  persistence: { enabled: true, size: 20Gi }
```

### 9.3 Production Deployment Script

```bash
#!/bin/bash
# scripts/deploy-production.sh
set -euo pipefail

NAMESPACE="workflow-platform"
RELEASE_NAME="workflow-platform"

log_info() { echo -e "\033[0;32m[INFO]\033[0m $1"; }
log_error() { echo -e "\033[0;31m[ERROR]\033[0m $1"; }

check_prerequisites() {
    log_info "Checking prerequisites..."
    command -v kubectl >/dev/null || { log_error "kubectl not found"; exit 1; }
    command -v helm >/dev/null || { log_error "helm not found"; exit 1; }
    kubectl cluster-info >/dev/null || { log_error "Cannot connect to cluster"; exit 1; }
}

deploy_application() {
    log_info "Deploying application..."
    helm upgrade --install "$RELEASE_NAME" ./helm/workflow-platform \
        --namespace "$NAMESPACE" \
        --create-namespace \
        --values ./helm/workflow-platform/values-production.yaml \
        --set image.api.tag="${IMAGE_TAG:-latest}" \
        --timeout 15m \
        --wait
}

verify_deployment() {
    log_info "Verifying deployment..."
    kubectl rollout status deployment/"${RELEASE_NAME}-api" -n "$NAMESPACE" --timeout=600s
    kubectl get pods -n "$NAMESPACE" -l "app.kubernetes.io/instance=$RELEASE_NAME"
}

main() {
    log_info "Starting production deployment"
    check_prerequisites
    deploy_application
    verify_deployment
    log_info "Deployment completed successfully! 🚀"
}

main "$@"
```

---

## 10. Development Setup Guide

### 10.1 Prerequisites

| Software | Version | Purpose |
|----------|---------|---------|
| **.NET SDK** | 8.0+ | Backend development |
| **Node.js** | 18.0+ | Frontend development |
| **Docker** | 24.0+ | Containerization |
| **Git** | 2.30+ | Version control |

### 10.2 Local Development Setup

```bash
# Clone and setup backend
git clone https://github.com/company/workflow-platform.git
cd workflow-platform
dotnet restore
dotnet build

# Setup user secrets
cd WorkflowPlatform.Api
dotnet user-secrets init
dotnet user-secrets set "JWT_SECRET" "development-jwt-secret-key-256-bits"
dotnet user-secrets set "DB_PASSWORD" "dev_password_123"

# Setup frontend
cd ../workflow-platform-frontend
npm install

# Start infrastructure
docker-compose up -d postgres redis rabbitmq

# Run migrations
cd ../WorkflowPlatform.Api
dotnet ef database update

# Start applications
dotnet run &
cd ../workflow-platform-frontend
npm run dev
```

### 10.3 Development Commands

#### Backend Commands

```bash
# Build and run with hot reload
dotnet watch run --project WorkflowPlatform.Api

# Generate migration
dotnet ef migrations add MigrationName --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.Api

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Frontend Commands

```bash
# Development server
npm run dev

# Type checking and linting
npm run type-check
npm run lint

# Testing
npm test
npm run test:coverage

# Build for production
npm run build
```

#### Docker Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api

# Clean up
docker-compose down -v
```

### 10.4 VS Code Configuration

```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/WorkflowPlatform.Api/bin/Debug/net8.0/WorkflowPlatform.Api.dll",
      "cwd": "${workspaceFolder}/WorkflowPlatform.Api",
      "env": { "ASPNETCORE_ENVIRONMENT": "Development" }
    },
    {
      "name": "Launch Next.js",
      "type": "node",
      "request": "launch",
      "cwd": "${workspaceFolder}/workflow-platform-frontend",
      "runtimeExecutable": "npm",
      "runtimeArgs": ["run", "dev"]
    }
  ]
}
```

---

## Appendix

### A. Troubleshooting Guide

| Issue | Solution |
|-------|----------|
| **Database connection fails** | Check PostgreSQL is running and connection string is correct |
| **JWT token errors** | Verify JWT_SECRET is at least 256 bits long |
| **SignalR connection fails** | Ensure CORS origins include frontend URL |
| **Frontend build fails** | Clear node_modules and reinstall dependencies |

### B. Security Checklist

- [ ] JWT secrets properly configured
- [ ] Database passwords are strong
- [ ] HTTPS enforced in production
- [ ] CORS origins restricted
- [ ] API rate limiting configured
- [ ] Input validation implemented
- [ ] Sensitive data encrypted
- [ ] Dependencies regularly updated

### C. Performance Monitoring

```bash
# Check resource usage
kubectl top pods -n workflow-platform

# View Prometheus metrics
curl http://localhost:9090/metrics

# Database performance
kubectl exec -it postgres-pod -- psql -U workflow_user -c "SELECT * FROM pg_stat_activity;"
```

### D. Deployment Checklist

- [ ] All tests pass in CI/CD
- [ ] Security scans complete
- [ ] Database migrations tested
- [ ] Environment variables configured
- [ ] Health checks working
- [ ] Monitoring configured
- [ ] Backup strategy in place
- [ ] Rollback plan tested
