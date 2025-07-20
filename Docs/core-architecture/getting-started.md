# Getting Started with Core Architecture

## 🎯 Current Status

✅ **Epic WOP-E001.1**: 100% Complete - All foundation components implemented  
✅ **Production Ready**: Enterprise-grade Clean Architecture established  
✅ **Developer Ready**: Complete development environment and tooling setup  
✅ **Documentation**: Comprehensive guides and examples available  

## Prerequisites

Before you start working with the Core Architecture, ensure you have the following installed:

### Required Software

- **.NET 8 SDK**: Version 8.0 or higher
  - Download from: <https://dotnet.microsoft.com/download>
  - Verify: `dotnet --version`

- **PostgreSQL**: Version 15 or higher
  - Download from: <https://www.postgresql.org/download/>
  - Alternative: Use Docker container

- **Visual Studio** or **VS Code**
  - Visual Studio 2022 (recommended for full experience)
  - VS Code with C# Dev Kit extension

### Optional Tools

- **Docker Desktop**: For containerized development
- **PostgreSQL Admin Tool**: pgAdmin, DBeaver, or similar
- **Postman**: For API testing
- **Git**: For version control

## Quick Setup (5 Minutes)

### Step 1: Clone and Navigate

```bash
# Clone the repository
git clone https://github.com/your-org/WorkFlowOchestrator.git
cd WorkFlowOchestrator

# Navigate to the solution
cd src
```

### Step 2: Database Setup

Choose one of the following options:

#### Option A: Local PostgreSQL

```bash
# Create database
createdb workflow_platform

# Set connection string (replace with your details)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
  "Host=localhost;Database=workflow_platform;Username=your_user;Password=your_password"
```

#### Option B: Docker PostgreSQL

```bash
# Run PostgreSQL in Docker
docker run --name postgres-wop -e POSTGRES_DB=workflow_platform 
  -e POSTGRES_USER=workflow -e POSTGRES_PASSWORD=workflow123 
  -p 5432:5432 -d postgres:15

# Set connection string for Docker
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
  "Host=localhost;Database=workflow_platform;Username=workflow;Password=workflow123"
```

### Step 3: Initialize Configuration

```bash
# Navigate to API project
cd WorkflowPlatform.API

# Initialize user secrets
dotnet user-secrets init

# Set required configuration
dotnet user-secrets set "Jwt:Secret" "your-super-secret-jwt-key-minimum-256-bits-long"
dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform.Dev"
dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform.Users"
```

### Step 4: Build and Run

```bash
# Navigate back to solution root
cd ..

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run database migrations
dotnet ef database update --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API

# Run the application
dotnet run --project WorkflowPlatform.API
```

### Step 5: Verify Setup

1. **API Documentation**: <https://localhost:7001/swagger>
2. **Health Checks**: <https://localhost:7001/health>
3. **Application Info**: <https://localhost:7001/info>

## Project Structure Deep Dive

### Solution Organization

```bash
WorkflowPlatform.sln
├── src/
│   ├── WorkflowPlatform.Domain/           # 💎 Pure business logic
│   │   ├── Common/                        
│   │   │   ├── Primitives/               # Base classes
│   │   │   ├── Interfaces/               # Domain contracts
│   │   │   ├── Exceptions/               # Domain exceptions
│   │   │   └── Enumerations/             # Domain enums
│   │   └── Workflows/                     
│   │       ├── Aggregates/               # WorkflowAggregate
│   │       ├── Entities/                 # Domain entities
│   │       ├── ValueObjects/             # Immutable objects
│   │       └── Events/                   # Domain events
│   │
│   ├── WorkflowPlatform.Application/      # 🧠 Use cases
│   │   ├── Common/                        
│   │   │   └── Interfaces/               # Application services
│   │   └── Workflows/                     
│   │       └── Commands/                 # CQRS commands
│   │
│   ├── WorkflowPlatform.Infrastructure/   # 🔧 External dependencies
│   │   ├── Persistence/                   
│   │   │   ├── WorkflowDbContext.cs      # EF Core context
│   │   │   └── Configurations/           # Entity configurations
│   │   ├── Repositories/                  # Data access
│   │   └── Services/                      # Infrastructure services
│   │
│   └── WorkflowPlatform.API/             # 🌐 Web interface
│       ├── Controllers/                   # REST endpoints
│       ├── Program.cs                     # Application startup
│       ├── appsettings.json              # Configuration
│       └── appsettings.Development.json  # Dev config
│
├── tests/                                 # 🧪 Test projects (future)
└── docs/                                  # 📚 Documentation
```

### Layer Responsibilities

#### 💎 **Domain Layer** (WorkflowPlatform.Domain)

**Purpose**: Contains the core business logic and rules

**Key Components**:

- **Entities**: Objects with identity that encapsulate business logic
- **Value Objects**: Immutable objects that describe characteristics
- **Aggregates**: Clusters of entities treated as a single unit
- **Domain Events**: Represent something significant that happened
- **Specifications**: Business rule expressions

**Example - Workflow Aggregate**:

```csharp
public class WorkflowAggregate : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public WorkflowDefinition Definition { get; private set; }
    public WorkflowStatus Status { get; private set; }

    public static WorkflowAggregate Create(string name, string description)
    {
        var workflow = new WorkflowAggregate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = WorkflowStatus.Draft
        };
        
        workflow.RaiseDomainEvent(new WorkflowCreatedEvent(workflow.Id, name));
        return workflow;
    }
}
```

#### 🧠 **Application Layer** (WorkflowPlatform.Application)

**Purpose**: Orchestrates business workflows and use cases

**Key Components**:

- **Commands**: Represent actions that change system state
- **Queries**: Represent data retrieval operations
- **Handlers**: Process commands and queries
- **DTOs**: Data transfer objects for external communication
- **Validators**: Input validation rules

**Example - Create Workflow Command**:

```csharp
public record CreateWorkflowCommand(
    string Name,
    string Description
) : IRequest<CreateWorkflowResult>;

public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, CreateWorkflowResult>
{
    private readonly IWorkflowRepository _repository;

    public async Task<CreateWorkflowResult> Handle(
        CreateWorkflowCommand request, 
        CancellationToken cancellationToken)
    {
        var workflow = WorkflowAggregate.Create(request.Name, request.Description);
        await _repository.AddAsync(workflow, cancellationToken);
        
        return new CreateWorkflowResult(workflow.Id);
    }
}
```

#### 🔧 **Infrastructure Layer** (WorkflowPlatform.Infrastructure)

**Purpose**: Implements external dependencies and persistence

**Key Components**:

- **DbContext**: Entity Framework database context
- **Repositories**: Data access implementations
- **External Services**: Email, file storage, etc.
- **Configurations**: Entity Framework configurations

**Example - Workflow Repository**:

```csharp
public class WorkflowRepository : IWorkflowRepository
{
    private readonly WorkflowDbContext _context;

    public async Task<WorkflowAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task AddAsync(WorkflowAggregate workflow, CancellationToken cancellationToken)
    {
        await _context.Workflows.AddAsync(workflow, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

#### 🌐 **Presentation Layer** (WorkflowPlatform.API)

**Purpose**: Provides the web API interface

**Key Components**:

- **Controllers**: REST API endpoints
- **DTOs**: Request/response models
- **Middleware**: Cross-cutting concerns
- **Configuration**: Startup and settings

**Example - Workflow Controller**:

```csharp
[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateWorkflowResult>> Create(
        CreateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkflowCommand(request.Name, request.Description);
        var result = await Mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## Configuration Management

### Environment Configuration

The application supports multiple environments with specific configurations:

#### Development (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=workflow_platform_dev;Username=dev_user;Password=dev_password"
  }
}
```

#### Production (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "#{DATABASE_CONNECTION_STRING}#"
  },
  "Jwt": {
    "Secret": "#{JWT_SECRET}#",
    "Issuer": "#{JWT_ISSUER}#",
    "Audience": "#{JWT_AUDIENCE}#"
  }
}
```

### User Secrets Management

For development, use user secrets to store sensitive configuration:

```bash
# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

# Set JWT configuration
dotnet user-secrets set "Jwt:Secret" "your-256-bit-secret-key"
dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform.Dev"
dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform.Users"

# Set additional services (optional)
dotnet user-secrets set "Email:ApiKey" "your-email-service-key"
dotnet user-secrets set "Storage:ConnectionString" "your-storage-connection"
```

## Development Workflow

### 1. Creating New Features

Follow this workflow when adding new features:

```bash
# 1. Create feature branch
git checkout -b feature/new-workflow-feature

# 2. Make changes following architecture patterns
# - Add domain logic to Domain layer
# - Add use cases to Application layer
# - Add data access to Infrastructure layer
# - Add API endpoints to Presentation layer

# 3. Run tests
dotnet test

# 4. Build solution
dotnet build

# 5. Commit changes
git add .
git commit -m "feat: add new workflow feature"

# 6. Push and create PR
git push origin feature/new-workflow-feature
```

### 2. Database Migrations

When you modify entities, create and apply migrations:

```bash
# Add migration
dotnet ef migrations add AddNewFeature --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API

# Update database
dotnet ef database update --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API

# Generate SQL script (optional)
dotnet ef migrations script --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API
```

### 3. Testing Your Changes

```bash
# Run unit tests
dotnet test --filter "Category=Unit"

# Run integration tests
dotnet test --filter "Category=Integration"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# View coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

## Common Development Tasks

### Adding a New Entity

1. **Create the entity in Domain layer**:

```csharp
// WorkflowPlatform.Domain/Workflows/Entities/Node.cs
public class Node : Entity<Guid>
{
    public string Name { get; private set; }
    public NodeType Type { get; private set; }
    public NodeConfiguration Configuration { get; private set; }

    private Node() { } // EF Constructor

    public static Node Create(string name, NodeType type, NodeConfiguration configuration)
    {
        return new Node
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Configuration = configuration
        };
    }
}
```

**Add to DbContext**:

```csharp
// WorkflowPlatform.Infrastructure/Persistence/WorkflowDbContext.cs
public DbSet<Node> Nodes { get; set; }
```

**Create entity configuration**:

```csharp
// WorkflowPlatform.Infrastructure/Persistence/Configurations/NodeConfiguration.cs
public class NodeConfiguration : IEntityTypeConfiguration<Node>
{
    public void Configure(EntityTypeBuilder<Node> builder)
    {
        builder.ToTable("Nodes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Name).HasMaxLength(255).IsRequired();
    }
}
```

**Add migration and update database**:

```bash
dotnet ef migrations add AddNodeEntity --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API
dotnet ef database update --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API
```

### Adding a New API Endpoint

1. **Create command/query in Application layer**:

```csharp
// WorkflowPlatform.Application/Nodes/Commands/CreateNodeCommand.cs
public record CreateNodeCommand(string Name, NodeType Type) : IRequest<CreateNodeResult>;
```

**Create handler**:

```csharp
public class CreateNodeHandler : IRequestHandler<CreateNodeCommand, CreateNodeResult>
{
    // Implementation
}
```

1. **Add controller endpoint**:

```csharp
// WorkflowPlatform.API/Controllers/NodesController.cs
[HttpPost]
public async Task<ActionResult<CreateNodeResult>> Create(CreateNodeRequest request)
{
    var command = new CreateNodeCommand(request.Name, request.Type);
    var result = await Mediator.Send(command);
    return Ok(result);
}
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Issues

**Problem**: Cannot connect to PostgreSQL database

**Solution**:

```bash
# Check PostgreSQL is running
pg_isready -h localhost -p 5432

# Verify connection string
dotnet user-secrets list

# Test connection manually
psql -h localhost -U your_user -d workflow_platform
```

#### 2. Migration Issues

**Problem**: Migration fails or database out of sync

**Solution**:

```bash
# Check current migration status
dotnet ef migrations list --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API

# Reset database (development only)
dotnet ef database drop --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API
dotnet ef database update --project WorkflowPlatform.Infrastructure --startup-project WorkflowPlatform.API
```

#### 3. Dependency Injection Issues

**Problem**: Service not registered or circular dependency

**Solution**:

```csharp
// Check service registration in DependencyInjection.cs files
// Ensure proper lifetimes (Scoped, Transient, Singleton)
// Check for circular dependencies in constructor parameters
```

#### 4. JWT Configuration Issues

**Problem**: Authentication fails or token validation errors

**Solution**:

```bash
# Ensure JWT secret is at least 256 bits (32 characters)
dotnet user-secrets set "Jwt:Secret" "your-super-long-secret-key-that-is-at-least-256-bits-long"

# Check issuer and audience match
dotnet user-secrets list
```

## Next Steps

### Recommended Learning Path

1. **[Architecture Guide](./architecture-guide.md)**: Deep dive into architectural patterns
2. **[Development Guide](./development-guide.md)**: Advanced development practices
3. **[Testing Guide](./testing-guide.md)**: Comprehensive testing strategies
4. **[Security Guide](./security-guide.md)**: Security implementation details
5. **[Performance Guide](./performance-guide.md)**: Optimization techniques

### Advanced Topics

- **Domain Events**: Implementing event-driven architecture
- **Caching Strategies**: Performance optimization with caching
- **Message Queues**: Asynchronous processing with RabbitMQ
- **Multi-tenancy**: Tenant isolation strategies

## Support

### Getting Help

- **Documentation**: Comprehensive guides in this folder
- **Code Examples**: Real implementations in the codebase
- **GitHub Issues**: Report bugs or request features
- **Team Chat**: Internal team communication channels

### Contributing

- **Code Standards**: Follow established patterns and conventions
- **Testing**: Maintain >90% test coverage
- **Documentation**: Update docs with any changes
- **Reviews**: All changes require code review

---

**Setup Status**: ✅ **READY TO DEVELOP**  
**Architecture**: 💎 **CLEAN ARCHITECTURE**  
**Quality**: ⭐ **ENTERPRISE-GRADE**  
**Next Step**: 📖 **[Architecture Guide](./architecture-guide.md)**
