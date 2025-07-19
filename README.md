# Workflow Orchestration Platform

![Workflow Platform Logo](https://via.placeholder.com/200x80/2563eb/ffffff?text=Workflow+Platform)

## Enterprise-grade workflow orchestration with visual design and real-time execution monitoring

[![Build Status](https://github.com/company/workflow-platform/workflows/CI%2FCD/badge.svg)](https://github.com/company/workflow-platform/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-14-black.svg)](https://nextjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED.svg)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326CE5.svg)](https://kubernetes.io/)

[Features](#-features) • [Quick Start](#-quick-start) • [API Documentation](#-api-documentation) • [Architecture](#-architecture) • [Contributing](#-contributing)

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Quick Start](#-quick-start)
- [Installation](#-installation)
- [Development Setup](#-development-setup)
- [Usage](#-usage)
- [API Documentation](#-api-documentation)
- [Deployment](#-deployment)
- [Configuration](#️-configuration)
- [Monitoring & Observability](#-monitoring--observability)
- [Security](#-security)
- [Performance](#-performance)
- [Testing](#-testing)
- [Contributing](#-contributing)
- [Troubleshooting](#-troubleshooting)
- [FAQ](#-faq)
- [Roadmap](#-roadmap)
- [License](#-license)
- [Support](#-support)

## 🌟 Overview

The Workflow Orchestration Platform is a comprehensive solution for designing, executing, and monitoring complex business workflows. Built with modern technologies and enterprise-grade patterns, it provides an intuitive visual interface for workflow creation while offering robust execution capabilities with real-time monitoring.

### Key Benefits

- **Visual Workflow Design**: Drag-and-drop interface with React Flow
- **Real-time Execution**: Live monitoring with SignalR integration
- **Enterprise Security**: JWT authentication, RBAC, and comprehensive audit logging
- **High Performance**: Supports 10,000+ concurrent workflow executions
- **Scalable Architecture**: Kubernetes-ready with horizontal scaling
- **Extensible Node System**: Built-in and custom node types with SDK

### Use Cases

- **Business Process Automation**: Automate complex business workflows
- **Data Pipeline Orchestration**: Coordinate data processing tasks
- **Integration Workflows**: Connect disparate systems and services
- **Approval Processes**: Multi-step approval and review workflows
- **Notification Systems**: Event-driven communication workflows
- **ETL Processes**: Extract, transform, and load data operations

## ✨ Features

### 🎨 Visual Workflow Designer

- **Drag-and-Drop Interface**: Intuitive workflow creation with React Flow
- **Real-time Collaboration**: Multi-user editing with conflict resolution
- **Node Library**: 25+ built-in node types for common operations
- **Custom Nodes**: SDK for developing custom node types
- **Version Control**: Workflow versioning with diff visualization
- **Template System**: Reusable workflow templates and marketplace

### ⚡ Execution Engine

- **Strategy Pattern**: Clean, extensible node execution architecture
- **Lifecycle Management**: Preprocessing → Execute → Postprocessing → Finalization
- **Parallel Execution**: Concurrent node execution with dependency resolution
- **Error Handling**: Circuit breakers, retry logic, and dead letter queues
- **Resource Management**: Automatic resource cleanup and optimization
- **State Persistence**: Reliable state management with PostgreSQL

### 📊 Real-time Monitoring

- **Real-time Dashboard**: Live execution monitoring with metrics
- **Prometheus Integration**: Comprehensive metrics collection
- **Grafana Dashboards**: Pre-built monitoring dashboards
- **Structured Logging**: Searchable logs with correlation IDs
- **Health Checks**: Kubernetes-ready health endpoints
- **Performance Analytics**: Execution time analysis and optimization

### 🔒 Security & Compliance

- **Multi-factor Authentication**: Enterprise SSO integration
- **Role-based Access Control**: Fine-grained permissions
- **Data Encryption**: At-rest and in-transit encryption
- **Audit Logging**: Complete audit trail for compliance
- **Secret Management**: Integration with external key vaults
- **Security Scanning**: Automated vulnerability assessment

### 🚀 High Performance

- **Horizontal Scaling**: Auto-scaling based on workload
- **Caching Strategy**: Redis-based distributed caching
- **Database Optimization**: Query optimization and connection pooling
- **Message Queues**: RabbitMQ for asynchronous processing
- **Load Balancing**: Intelligent request distribution
- **Resource Limits**: Configurable resource constraints

## 🛠 Technology Stack

### Backend Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 8.0 | Web API framework |
| **ASP.NET Core** | 8.0 | Web API and gRPC services |
| **Entity Framework Core** | 8.0 | ORM for database operations |
| **PostgreSQL** | 15+ | Primary database |
| **Redis** | 7+ | Caching and session storage |
| **RabbitMQ** | 3.12+ | Message queuing |
| **MassTransit** | 8.1+ | Message bus abstraction |
| **SignalR** | 8.0 | Real-time communications |
| **gRPC** | 2.60+ | High-performance RPC |
| **MediatR** | 12.2+ | CQRS and mediator pattern |
| **FluentValidation** | 11.8+ | Input validation |
| **AutoMapper** | 12.0+ | Object mapping |
| **Serilog** | 3.1+ | Structured logging |
| **Prometheus** | 8.2+ | Metrics collection |

### Frontend Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **Next.js** | 14+ | React framework |
| **React** | 18+ | UI library |
| **TypeScript** | 5.0+ | Type safety |
| **Tailwind CSS** | 3.4+ | Styling framework |
| **shadcn/ui** | Latest | UI component library |
| **React Flow** | 11+ | Workflow visualization |
| **Zustand** | 4.4+ | State management |
| **React Query** | 5.0+ | Server state management |
| **React Hook Form** | 7.48+ | Form handling |
| **Zod** | 3.22+ | Schema validation |
| **Recharts** | 2.8+ | Data visualization |
| **Lucide React** | 0.263+ | Icon library |

### Infrastructure

| Technology | Version | Purpose |
|------------|---------|---------|
| **Docker** | 24.0+ | Containerization |
| **Kubernetes** | 1.25+ | Container orchestration |
| **Helm** | 3.10+ | Kubernetes package manager |
| **GitHub Actions** | - | CI/CD pipeline |
| **GitHub Container Registry** | - | Container registry |
| **Prometheus** | 2.45+ | Metrics collection |
| **Grafana** | 10.0+ | Monitoring dashboards |
| **Nginx** | 1.24+ | Reverse proxy and load balancer |

## 🏗 Architecture

### System Architecture

```mermaid
graph TB
    subgraph "Frontend Layer"
        UI[Next.js UI]
        WD[Workflow Designer]
        ED[Execution Dashboard]
    end
    
    subgraph "API Gateway"
        NGINX[Nginx Load Balancer]
    end
    
    subgraph "Backend Services"
        API[REST API]
        GRPC[gRPC Services]
        HUB[SignalR Hubs]
        ENGINE[Execution Engine]
    end
    
    subgraph "Message Layer"
        RMQ[RabbitMQ]
        REDIS[Redis Cache]
    end
    
    subgraph "Data Layer"
        PG[(PostgreSQL)]
        LOGS[(Log Storage)]
    end
    
    subgraph "Monitoring"
        PROM[Prometheus]
        GRAF[Grafana]
    end
    
    UI --> NGINX
    WD --> NGINX
    ED --> NGINX
    
    NGINX --> API
    NGINX --> GRPC
    NGINX --> HUB
    
    API --> ENGINE
    GRPC --> ENGINE
    HUB --> ENGINE
    
    ENGINE --> RMQ
    ENGINE --> REDIS
    ENGINE --> PG
    
    API --> PROM
    ENGINE --> LOGS
    
    PROM --> GRAF
```

### Clean Architecture Layers

```mermaid
graph LR
    subgraph "Domain Layer"
        AGG[Aggregates]
        ENT[Entities]
        VO[Value Objects]
        DE[Domain Events]
    end
    
    subgraph "Application Layer"
        CH[Command Handlers]
        QH[Query Handlers]
        SVC[Application Services]
        INT[Interfaces]
    end
    
    subgraph "Infrastructure Layer"
        REPO[Repositories]
        EXT[External Services]
        DB[Database Context]
        MSG[Message Handlers]
    end
    
    subgraph "Presentation Layer"
        CTRL[Controllers]
        HUB[SignalR Hubs]
        GRPC[gRPC Services]
    end
    
    CTRL --> CH
    CTRL --> QH
    HUB --> SVC
    GRPC --> CH
    
    CH --> AGG
    QH --> INT
    SVC --> ENT
    
    REPO --> DB
    EXT --> MSG
    
    AGG --> VO
    ENT --> DE
```

### Node Execution Strategy Pattern

```mermaid
sequenceDiagram
    participant EE as Execution Engine
    participant SF as Strategy Factory
    participant NS as Node Strategy
    participant DB as Database
    participant EXT as External Service
    
    EE->>SF: GetStrategy(nodeType)
    SF->>NS: Create Strategy Instance
    
    EE->>NS: PreprocessAsync()
    NS->>DB: Validate Configuration
    NS->>EE: ValidationResult
    
    EE->>NS: ExecuteAsync()
    NS->>EXT: Perform Operation
    EXT->>NS: Operation Result
    NS->>EE: ExecutionResult
    
    EE->>NS: PostprocessAsync()
    NS->>NS: Transform Output
    NS->>EE: TransformedResult
    
    EE->>NS: FinalizationAsync()
    NS->>DB: Persist State
    NS->>EE: Completion
```

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 30-Second Setup

```bash
# Clone the repository
git clone https://github.com/company/workflow-platform.git
cd workflow-platform

# Start infrastructure services
docker-compose up -d postgres redis rabbitmq

# Setup backend
cd WorkflowPlatform.Api
dotnet user-secrets set "JWT_SECRET" "your-super-secret-jwt-key-that-is-at-least-256-bits-long"
dotnet run

# Setup frontend (new terminal)
cd workflow-platform-frontend
npm install
npm run dev
```

**Access the application:**

- Frontend: <http://localhost:3000>
- API: <http://localhost:5001>
- API Documentation: <http://localhost:5001/swagger>

## 📦 Installation

### Development Environment

#### 1. Clone Repository

```bash
git clone https://github.com/company/workflow-platform.git
cd workflow-platform
```

#### 2. Infrastructure Setup

```bash
# Start required services
docker-compose up -d postgres redis rabbitmq prometheus grafana

# Verify services are running
docker-compose ps
```

#### 3. Backend Setup

```bash
# Navigate to API project
cd WorkflowPlatform.Api

# Set up user secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=workflow_dev;Username=workflow_user;Password=dev_password"
dotnet user-secrets set "JWT_SECRET" "your-super-secret-jwt-key-that-is-at-least-256-bits-long"
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379"
dotnet user-secrets set "RabbitMQ:ConnectionString" "amqp://guest:guest@localhost:5672/"

# Restore packages and run migrations
dotnet restore
dotnet ef database update

# Start the API
dotnet run
```

#### 4. Frontend Setup

```bash
# Navigate to frontend project
cd workflow-platform-frontend

# Install dependencies
npm install

# Set up environment variables
cp .env.example .env.local

# Start development server
npm run dev
```

### Production Deployment

#### Production Docker Setup

```bash
# Build images
docker build -t workflow-platform/api:latest -f WorkflowPlatform.Api/Dockerfile .
docker build -t workflow-platform/frontend:latest -f workflow-platform-frontend/Dockerfile ./workflow-platform-frontend

# Run with docker-compose
docker-compose -f docker-compose.prod.yml up -d
```

#### Production Kubernetes Setup

```bash
# Install with Helm
helm repo add workflow-platform https://charts.workflow-platform.com
helm install workflow-platform workflow-platform/workflow-platform \
  --namespace workflow-platform \
  --create-namespace \
  --values values-production.yaml
```

## 🔧 Development Setup

### VS Code Configuration

Install recommended extensions:

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-typescript-next",
    "github.copilot",
    "ms-kubernetes-tools.vscode-kubernetes-tools"
  ]
}
```

### Environment Variables

#### Backend Environment Variables

```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development
export JWT_SECRET="your-jwt-secret"
export DATABASE_CONNECTION_STRING="Host=localhost;Database=workflow_dev;Username=workflow_user;Password=dev_password"
export REDIS_CONNECTION_STRING="localhost:6379"
export RABBITMQ_CONNECTION_STRING="amqp://guest:guest@localhost:5672/"

# Production
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET="${JWT_SECRET}"
export DATABASE_CONNECTION_STRING="${DATABASE_CONNECTION_STRING}"
export REDIS_CONNECTION_STRING="${REDIS_CONNECTION_STRING}"
export RABBITMQ_CONNECTION_STRING="${RABBITMQ_CONNECTION_STRING}"
```

#### Frontend (Next.js)

```bash
# .env.local
NEXT_PUBLIC_API_BASE_URL=http://localhost:5001
NEXT_PUBLIC_GRPC_BASE_URL=http://localhost:5001
NEXT_PUBLIC_SIGNALR_BASE_URL=http://localhost:5001
NEXT_PUBLIC_APP_NAME="Workflow Platform"
```

### Development Commands

#### Backend

```bash
# Build and test
dotnet build
dotnet test

# Run with hot reload
dotnet watch run

# Entity Framework migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Code formatting
dotnet format

# Security scan
dotnet list package --vulnerable
```

#### Frontend Development

```bash
# Development server
npm run dev

# Type checking
npm run type-check

# Linting
npm run lint
npm run lint:fix

# Testing
npm test
npm run test:coverage

# Build for production
npm run build
npm start
```

## 💻 Usage

### Creating Your First Workflow

1. **Open the Workflow Designer**
   - Navigate to <http://localhost:3000>
   - Click "Create New Workflow"

2. **Design Your Workflow**
   - Drag nodes from the palette
   - Connect nodes to define flow
   - Configure node properties

3. **Execute the Workflow**
   - Click "Execute" button
   - Monitor real-time progress
   - View execution results

### Built-in Node Types

#### Integration Nodes

- **HTTP Request**: Make REST API calls
- **Database Query**: Execute SQL queries
- **Email Notification**: Send emails via SMTP
- **File Operations**: Read/write files
- **FTP Transfer**: Upload/download files

#### Logic Nodes

- **Conditional**: If-then-else logic
- **Loop**: Iterate over collections
- **Delay**: Add time delays
- **Variable**: Store and retrieve data
- **Transform**: Data transformation

#### Utility Nodes

- **Logger**: Log messages
- **Webhook**: Receive HTTP callbacks
- **Timer**: Schedule execution
- **Counter**: Track executions
- **Validator**: Data validation

### Creating Custom Nodes

```csharp
public class CustomNodeStrategy : BaseNodeExecutionStrategy
{
    public override string NodeType => "CustomNode";

    public override async Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context, 
        CancellationToken cancellationToken)
    {
        // Your custom logic here
        var config = context.GetConfiguration<CustomNodeConfig>();
        var result = await ProcessDataAsync(config, cancellationToken);
        
        return NodeExecutionResult.Success(result);
    }
}
```

## 📚 API Documentation

### REST API Endpoints

#### Workflows

```http
GET    /api/workflows              # List workflows
POST   /api/workflows              # Create workflow
GET    /api/workflows/{id}         # Get workflow
PUT    /api/workflows/{id}         # Update workflow
DELETE /api/workflows/{id}         # Delete workflow
POST   /api/workflows/{id}/execute # Execute workflow
```

#### Executions

```http
GET    /api/executions             # List executions
GET    /api/executions/{id}        # Get execution
DELETE /api/executions/{id}        # Cancel execution
GET    /api/executions/{id}/logs   # Get execution logs
```

#### Nodes

```http
GET    /api/nodes/types            # List node types
GET    /api/nodes/types/{type}     # Get node type details
POST   /api/nodes/validate         # Validate node configuration
```

### gRPC Services

```protobuf
service WorkflowExecution {
  rpc ExecuteWorkflow(ExecuteWorkflowRequest) returns (stream ExecutionUpdate);
  rpc CancelExecution(CancelExecutionRequest) returns (CancelExecutionResponse);
  rpc GetExecutionStatus(GetExecutionStatusRequest) returns (ExecutionStatusResponse);
}
```

### SignalR Hubs

#### Workflow Hub

```typescript
// Connect to hub
const connection = new HubConnectionBuilder()
  .withUrl("/hubs/workflow")
  .build();

// Listen for execution updates
connection.on("ExecutionUpdate", (update) => {
  console.log("Execution update:", update);
});

// Join workflow execution room
connection.invoke("JoinExecution", executionId);
```

### Authentication

All API endpoints require JWT authentication:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Getting a Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password"
}
```

Response:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2024-12-31T23:59:59Z",
  "user": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "roles": ["User"]
  }
}
```

## 🚀 Deployment

### Docker Deployment

#### Single Node Deployment

```bash
# Clone repository
git clone https://github.com/company/workflow-platform.git
cd workflow-platform

# Start services
docker-compose -f docker-compose.prod.yml up -d

# Check status
docker-compose ps
```

#### Multi-Node Deployment

```yaml
# docker-swarm.yml
version: '3.8'
services:
  api:
    image: ghcr.io/company/workflow-platform/api:latest
    deploy:
      replicas: 3
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

```bash
# Deploy to Docker Swarm
docker stack deploy -c docker-swarm.yml workflow-platform
```

### Kubernetes Deployment

#### Using Helm Chart

```bash
# Add Helm repository
helm repo add workflow-platform https://charts.workflow-platform.com
helm repo update

# Install with custom values
helm install workflow-platform workflow-platform/workflow-platform \
  --namespace workflow-platform \
  --create-namespace \
  --values values-production.yaml \
  --wait --timeout=15m
```

#### values-production.yaml

```yaml
api:
  replicaCount: 3
  image:
    repository: ghcr.io/company/workflow-platform/api
    tag: "v1.0.0"
  resources:
    requests:
      memory: "256Mi"
      cpu: "250m"
    limits:
      memory: "512Mi"
      cpu: "500m"
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10
    targetCPUUtilizationPercentage: 70

frontend:
  replicaCount: 2
  image:
    repository: ghcr.io/company/workflow-platform/frontend
    tag: "v1.0.0"

postgresql:
  enabled: true
  auth:
    username: "workflow_user"
    database: "workflow_platform"
  primary:
    persistence:
      enabled: true
      size: 100Gi

redis:
  enabled: true
  auth:
    enabled: true
  master:
    persistence:
      enabled: true
      size: 10Gi

ingress:
  enabled: true
  className: "nginx"
  hosts:
    - host: workflow.company.com
      paths:
        - path: /
          pathType: Prefix
```

#### Manual Kubernetes Deployment

```bash
# Apply manifests
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/ingress.yaml

# Check deployment status
kubectl get pods -n workflow-platform
kubectl get services -n workflow-platform
```

### Cloud Provider Specific

#### AWS EKS

```bash
# Create EKS cluster
eksctl create cluster --name workflow-platform --region us-west-2 --nodes 3

# Install AWS Load Balancer Controller
kubectl apply -k "github.com/aws/eks-charts/stable/aws-load-balancer-controller//crds?ref=master"

# Deploy application
helm install workflow-platform ./helm/workflow-platform \
  --set ingress.annotations."kubernetes\.io/ingress\.class"=alb
```

#### Azure AKS

```bash
# Create AKS cluster
az aks create --resource-group workflow-platform-rg --name workflow-platform --node-count 3

# Get credentials
az aks get-credentials --resource-group workflow-platform-rg --name workflow-platform

# Deploy application
helm install workflow-platform ./helm/workflow-platform \
  --set ingress.className=azure-application-gateway
```

#### Google GKE

```bash
# Create GKE cluster
gcloud container clusters create workflow-platform --num-nodes=3

# Get credentials
gcloud container clusters get-credentials workflow-platform

# Deploy application
helm install workflow-platform ./helm/workflow-platform \
  --set ingress.className=gce
```

## ⚙️ Configuration

### Application Configuration

#### Backend Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "WorkflowPlatform": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=workflow_platform;Username=workflow_user;Password=password",
    "Redis": "localhost:6379",
    "RabbitMQ": "amqp://guest:guest@localhost:5672/"
  },
  "Jwt": {
    "Secret": "your-super-secret-jwt-key-that-is-at-least-256-bits-long",
    "Issuer": "WorkflowPlatform",
    "Audience": "WorkflowPlatform.Users",
    "ExpirationHours": 24
  },
  "WorkflowEngine": {
    "MaxConcurrentExecutions": 1000,
    "DefaultTimeoutMinutes": 30,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 5
  },
  "Features": {
    "EnableCollaboration": true,
    "EnableMarketplace": true,
    "EnableAIOptimization": false
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "RequestsPerMinute": 100,
    "BurstSize": 200
  }
}
```

#### Frontend Configuration (next.config.js)

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    appDir: true
  },
  env: {
    CUSTOM_KEY: process.env.CUSTOM_KEY,
  },
  images: {
    domains: ['localhost', 'api.workflow-platform.com'],
  },
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/:path*`,
      },
    ];
  },
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'DENY',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'Referrer-Policy',
            value: 'strict-origin-when-cross-origin',
          },
        ],
      },
    ];
  },
};

module.exports = nextConfig;
```

### Environment-Specific Configuration

#### Development

```bash
# .env.development
ASPNETCORE_ENVIRONMENT=Development
DATABASE_CONNECTION_STRING=Host=localhost;Database=workflow_dev;Username=workflow_user;Password=dev_password
REDIS_CONNECTION_STRING=localhost:6379
RABBITMQ_CONNECTION_STRING=amqp://guest:guest@localhost:5672/
LOG_LEVEL=Information
ENABLE_SWAGGER=true
CORS_ORIGINS=http://localhost:3000,https://localhost:3000
```

#### Staging

```bash
# .env.staging
ASPNETCORE_ENVIRONMENT=Staging
DATABASE_CONNECTION_STRING=${DATABASE_CONNECTION_STRING}
REDIS_CONNECTION_STRING=${REDIS_CONNECTION_STRING}
RABBITMQ_CONNECTION_STRING=${RABBITMQ_CONNECTION_STRING}
LOG_LEVEL=Information
ENABLE_SWAGGER=true
CORS_ORIGINS=https://staging.workflow-platform.com
```

#### Production

```bash
# .env.production
ASPNETCORE_ENVIRONMENT=Production
DATABASE_CONNECTION_STRING=${DATABASE_CONNECTION_STRING}
REDIS_CONNECTION_STRING=${REDIS_CONNECTION_STRING}
RABBITMQ_CONNECTION_STRING=${RABBITMQ_CONNECTION_STRING}
LOG_LEVEL=Warning
ENABLE_SWAGGER=false
CORS_ORIGINS=https://workflow-platform.com
```

### Database Configuration

#### PostgreSQL Optimization

```sql
-- postgresql.conf optimizations
shared_buffers = '256MB'
effective_cache_size = '1GB'
maintenance_work_mem = '64MB'
checkpoint_completion_target = 0.9
wal_buffers = '16MB'
default_statistics_target = 100
random_page_cost = 1.1
effective_io_concurrency = 200

-- Connection pooling
max_connections = 200
```

#### Redis Database Configuration

```conf
# redis.conf
maxmemory 1gb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000
tcp-keepalive 300
timeout 300
```

### Message Queue Configuration

#### RabbitMQ Setup

```bash
# Enable management plugin
rabbitmq-plugins enable rabbitmq_management

# Create users and virtual hosts
rabbitmqctl add_user workflow_user password
rabbitmqctl add_vhost workflow_platform
rabbitmqctl set_permissions -p workflow_platform workflow_user ".*" ".*" ".*"
```

#### Queue Configuration

```json
{
  "RabbitMQ": {
    "ConnectionString": "amqp://workflow_user:password@localhost:5672/workflow_platform",
    "Queues": {
      "WorkflowExecution": {
        "Durable": true,
        "AutoDelete": false,
        "PrefetchCount": 10
      },
      "NodeExecution": {
        "Durable": true,
        "AutoDelete": false,
        "PrefetchCount": 50
      },
      "DeadLetter": {
        "Durable": true,
        "AutoDelete": false,
        "MessageTtl": 86400000
      }
    }
  }
}
```

## 📊 Monitoring & Observability

### Metrics Collection

#### Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "workflow_alerts.yml"

scrape_configs:
  - job_name: 'workflow-api'
    static_configs:
      - targets: ['localhost:5001']
    metrics_path: '/metrics'
    scrape_interval: 10s

  - job_name: 'postgresql'
    static_configs:
      - targets: ['localhost:9187']

  - job_name: 'redis'
    static_configs:
      - targets: ['localhost:9121']

  - job_name: 'rabbitmq'
    static_configs:
      - targets: ['localhost:15692']

alerting:
  alertmanagers:
    - static_configs:
        - targets: ['localhost:9093']
```

#### Custom Metrics

```csharp
// Workflow execution metrics
private static readonly Counter WorkflowExecutions = Metrics
    .CreateCounter("workflow_executions_total", "Total workflow executions", "status", "workflow_type");

private static readonly Histogram ExecutionDuration = Metrics
    .CreateHistogram("workflow_execution_duration_seconds", "Workflow execution duration");

private static readonly Gauge ActiveExecutions = Metrics
    .CreateGauge("workflow_active_executions", "Currently active workflow executions");

// API metrics
private static readonly Counter ApiRequests = Metrics
    .CreateCounter("api_requests_total", "Total API requests", "method", "endpoint", "status");

private static readonly Histogram RequestDuration = Metrics
    .CreateHistogram("api_request_duration_seconds", "API request duration");
```

### Grafana Dashboards

#### Workflow Platform Overview Dashboard

```json
{
  "dashboard": {
    "title": "Workflow Platform Overview",
    "panels": [
      {
        "title": "Workflow Executions",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(workflow_executions_total[5m])",
            "legendFormat": "Executions/sec"
          }
        ]
      },
      {
        "title": "Active Executions",
        "type": "stat",
        "targets": [
          {
            "expr": "workflow_active_executions",
            "legendFormat": "Active"
          }
        ]
      },
      {
        "title": "Success Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(workflow_executions_total{status=\"success\"}[5m]) / rate(workflow_executions_total[5m]) * 100",
            "legendFormat": "Success %"
          }
        ]
      }
    ]
  }
}
```

### Alerting Rules

#### Prometheus Alerts

```yaml
# workflow_alerts.yml
groups:
  - name: workflow_platform
    rules:
      - alert: HighWorkflowFailureRate
        expr: rate(workflow_executions_total{status="failed"}[5m]) / rate(workflow_executions_total[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: High workflow failure rate detected
          description: "Workflow failure rate is {{ $value }}% over the last 5 minutes"

      - alert: WorkflowExecutionTimeout
        expr: histogram_quantile(0.95, workflow_execution_duration_seconds) > 300
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: Workflow executions taking too long
          description: "95th percentile execution time is {{ $value }} seconds"

      - alert: APIHighLatency
        expr: histogram_quantile(0.95, api_request_duration_seconds) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: API experiencing high latency
          description: "95th percentile API response time is {{ $value }} seconds"

      - alert: DatabaseConnectionsHigh
        expr: pg_stat_activity_count > 180
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: High database connection count
          description: "Database has {{ $value }} active connections"
```

### Log Management

#### Structured Logging Configuration

```csharp
// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("Application", "WorkflowPlatform")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(new JsonFormatter(), "logs/workflow-platform-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "workflow-platform-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();
```

#### Log Correlation

```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers.Add("X-Correlation-ID", correlationId);
            await _next(context);
        }
    }
}
```

## 🔒 Security

### Authentication & Authorization

#### JWT Configuration

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Allow SignalR to get token from query string
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
```

#### Role-Based Access Control

```csharp
// Authorization policies
services.AddAuthorization(options =>
{
    options.AddPolicy("WorkflowRead", policy =>
        policy.RequireClaim("permission", "workflow.read"));
    
    options.AddPolicy("WorkflowWrite", policy =>
        policy.RequireClaim("permission", "workflow.write"));
    
    options.AddPolicy("WorkflowExecute", policy =>
        policy.RequireClaim("permission", "workflow.execute"));
    
    options.AddPolicy("WorkflowAdmin", policy =>
        policy.RequireRole("Administrator")
              .RequireClaim("permission", "workflow.admin"));
});

// Controller usage
[Authorize(Policy = "WorkflowExecute")]
[HttpPost("{id}/execute")]
public async Task<IActionResult> ExecuteWorkflow(Guid id, [FromBody] ExecuteWorkflowRequest request)
{
    // Implementation
}
```

### Data Protection

#### Encryption Configuration

```csharp
// Data protection configuration
services.AddDataProtection()
    .SetApplicationName("WorkflowPlatform")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .ProtectKeysWithCertificate(GetCertificate())
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Secret encryption service
public class SecretEncryptionService
{
    private readonly IDataProtector _protector;

    public SecretEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("WorkflowPlatform.Secrets");
    }

    public string Encrypt(string plaintext)
    {
        return _protector.Protect(plaintext);
    }

    public string Decrypt(string ciphertext)
    {
        return _protector.Unprotect(ciphertext);
    }
}
```

#### Database Security

```sql
-- Row Level Security for multi-tenancy
ALTER TABLE workflows ENABLE ROW LEVEL SECURITY;

CREATE POLICY workflow_isolation ON workflows
    FOR ALL TO workflow_user
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- Audit logging
CREATE TABLE audit_log (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(50) NOT NULL,
    operation VARCHAR(10) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    user_id UUID NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $
BEGIN
    INSERT INTO audit_log (table_name, operation, old_values, new_values, user_id)
    VALUES (
        TG_TABLE_NAME,
        TG_OP,
        CASE WHEN TG_OP = 'DELETE' THEN row_to_json(OLD) ELSE NULL END,
        CASE WHEN TG_OP IN ('INSERT', 'UPDATE') THEN row_to_json(NEW) ELSE NULL END,
        current_setting('app.current_user_id')::uuid
    );
    RETURN COALESCE(NEW, OLD);
END;
$ LANGUAGE plpgsql;
```

### Network Security

#### CORS Configuration

```csharp
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins(configuration.GetSection("CORS:AllowedOrigins").Get<string[]>())
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "X-Correlation-ID")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
    });
});
```

#### Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("DefaultPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 50;
    });

    options.AddSlidingWindowLimiter("ExecutionPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
});

// Usage
[EnableRateLimiting("ExecutionPolicy")]
[HttpPost("{id}/execute")]
public async Task<IActionResult> ExecuteWorkflow(Guid id, [FromBody] ExecuteWorkflowRequest request)
{
    // Implementation
}
```

### Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    
    await next();
});
```

## ⚡ Performance

### Performance Targets

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| **API Response Time** | 95th percentile <200ms | Prometheus histogram |
| **Workflow Execution Startup** | <1 second | Custom metrics |
| **Concurrent Executions** | 10,000+ sustained | Load testing |
| **Database Query Time** | <50ms average | PostgreSQL logs |
| **Memory Usage** | <2GB per instance | Container metrics |
| **CPU Usage** | <70% under load | Container metrics |

### Database Optimization

#### Indexes

```sql
-- Workflow execution queries
CREATE INDEX CONCURRENTLY idx_workflow_instances_status_started 
    ON workflow_instances(status, started_at) 
    WHERE status IN ('Running', 'Pending');

CREATE INDEX CONCURRENTLY idx_node_executions_workflow_instance 
    ON node_executions(workflow_instance_id, started_at);

-- Audit queries
CREATE INDEX CONCURRENTLY idx_audit_log_timestamp_user 
    ON audit_log(timestamp DESC, user_id);

-- Partial indexes for active workflows
CREATE INDEX CONCURRENTLY idx_workflows_active 
    ON workflows(name, updated_at) 
    WHERE is_active = true;
```

#### Query Optimization

```csharp
// Efficient pagination
public async Task<PagedResult<WorkflowInstance>> GetWorkflowInstancesAsync(
    int pageNumber, 
    int pageSize,
    WorkflowStatus? status = null,
    CancellationToken cancellationToken = default)
{
    var query = _context.WorkflowInstances
        .AsNoTracking()
        .Include(w => w.WorkflowDefinition)
        .Where(w => status == null || w.Status == status)
        .OrderByDescending(w => w.StartedAt);

    var totalCount = await query.CountAsync(cancellationToken);
    
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return new PagedResult<WorkflowInstance>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}

// Bulk operations
public async Task UpdateNodeStatusesAsync(
    IEnumerable<NodeStatusUpdate> updates,
    CancellationToken cancellationToken = default)
{
    const int batchSize = 100;
    var batches = updates.Chunk(batchSize);

    foreach (var batch in batches)
    {
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE node_executions 
              SET status = data.status, 
                  completed_at = data.completed_at,
                  output_data = data.output_data
              FROM (VALUES {0}) AS data(id, status, completed_at, output_data)
              WHERE node_executions.id = data.id::uuid",
            string.Join(",", batch.Select(FormatUpdateValues)),
            cancellationToken);
    }
}
```

### Caching Strategy

#### Redis Caching Setup

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "WorkflowPlatform";
});

// Distributed caching service
public class WorkflowCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<WorkflowCacheService> _logger;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
        if (cachedValue == null) return default;

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        var serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
```

#### Cache-Aside Pattern

```csharp
public async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(
    Guid workflowId, 
    CancellationToken cancellationToken = default)
{
    var cacheKey = $"workflow_definition:{workflowId}";
    
    // Try cache first
    var cached = await _cacheService.GetAsync<WorkflowDefinition>(cacheKey, cancellationToken);
    if (cached != null)
    {
        return cached;
    }

    // Cache miss - get from database
    var workflow = await _repository.GetByIdAsync(workflowId, cancellationToken);
    if (workflow != null)
    {
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, workflow, TimeSpan.FromMinutes(5), cancellationToken);
    }

    return workflow;
}
```

### Frontend Performance

#### Code Splitting

```typescript
// Dynamic imports for route-based code splitting
const WorkflowDesigner = dynamic(() => import('@/components/workflow/WorkflowDesigner'), {
  loading: () => <LoadingSpinner />,
  ssr: false
});

const ExecutionDashboard = dynamic(() => import('@/components/execution/ExecutionDashboard'), {
  loading: () => <LoadingSpinner />
});

// Component-based code splitting
const NodePalette = lazy(() => import('./NodePalette'));
const NodePropertyPanel = lazy(() => import('./NodePropertyPanel'));
```

#### Performance Optimization

```typescript
// Memoization for expensive calculations
const processedNodes = useMemo(() => {
  return nodes.map(node => ({
    ...node,
    isValid: validateNode(node),
    position: calculateOptimalPosition(node)
  }));
}, [nodes, validationRules]);

// Virtualization for large lists
import { FixedSizeList as List } from 'react-window';

const WorkflowList: React.FC<{ workflows: Workflow[] }> = ({ workflows }) => {
  const Row = ({ index, style }: { index: number; style: React.CSSProperties }) => (
    <div style={style}>
      <WorkflowListItem workflow={workflows[index]} />
    </div>
  );

  return (
    <List
      height={600}
      itemCount={workflows.length}
      itemSize={80}
      width="100%"
    >
      {Row}
    </List>
  );
};

// Debounced API calls
const debouncedSave = useCallback(
  debounce(async (workflowData: WorkflowDefinition) => {
    await workflowApi.updateWorkflow(workflowData.id, workflowData);
  }, 1000),
  []
);
```

## 🧪 Testing

### Testing Strategy

#### Test Pyramid

```text
                    E2E Tests (5%)
                 ┌─────────────────┐
                 │  Browser Tests  │
                 │  API Tests      │
                 └─────────────────┘
                
              Integration Tests (15%)
           ┌─────────────────────────┐
           │  Controller Tests       │
           │  Repository Tests       │
           │  Service Tests          │
           └─────────────────────────┘
           
           Unit Tests (80%)
    ┌─────────────────────────────────┐
    │  Domain Logic Tests             │
    │  Business Rule Tests            │
    │  Component Tests                │
    └─────────────────────────────────┘
```

### Backend Testing

#### Unit Tests

```csharp
public class WorkflowAggregateTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateWorkflow()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        var createdBy = Guid.NewGuid();

        // Act
        var workflow = WorkflowAggregate.Create(name, description, createdBy);

        // Assert
        workflow.Should().NotBeNull();
        workflow.Name.Should().Be(name);
        workflow.Description.Should().Be(description);
        workflow.CreatedBy.Should().Be(createdBy);
        workflow.IsActive.Should().BeTrue();
        workflow.UncommittedEvents.Should().HaveCount(1);
        workflow.UncommittedEvents.First().Should().BeOfType<WorkflowCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var description = "Test Description";
        var createdBy = Guid.NewGuid();

        // Act & Assert
        var action = () => WorkflowAggregate.Create(invalidName, description, createdBy);
        action.Should().Throw<ArgumentException>();
    }
}

public class HttpRequestNodeStrategyTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<HttpRequestNodeStrategy>> _mockLogger;
    private readonly Mock<IMetricsCollector> _mockMetrics;
    private readonly HttpRequestNodeStrategy _strategy;

    public HttpRequestNodeStrategyTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<HttpRequestNodeStrategy>>();
        _mockMetrics = new Mock<IMetricsCollector>();
        _strategy = new HttpRequestNodeStrategy(_mockHttpClient.Object, _mockLogger.Object, _mockMetrics.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var context = CreateValidExecutionContext();
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { result = "success" }))
        };

        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _strategy.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OutputData.Should().Contain("success");
        
        _mockHttpClient.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _mockMetrics.Verify(
            x => x.RecordNodeExecution(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()), 
            Times.Once);
    }
}
```

#### Integration Tests

```csharp
public class WorkflowApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WorkflowApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add test database
                services.AddDbContext<WorkflowDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateWorkflow_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateWorkflowRequest
        {
            Name = "Test Workflow",
            Description = "Integration test workflow",
            Definition = new WorkflowDefinition
            {
                Nodes = new List<WorkflowNode>(),
                Edges = new List<WorkflowEdge>()
            }
        };

        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var workflow = await response.Content.ReadFromJsonAsync<WorkflowResponse>();
        workflow.Should().NotBeNull();
        workflow!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithValidWorkflow_ShouldReturnExecutionResult()
    {
        // Arrange
        var workflow = await CreateTestWorkflowAsync();
        var executeRequest = new ExecuteWorkflowRequest
        {
            InputData = new { testValue = "test" }
        };

        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workflows/{workflow.Id}/execute", executeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WorkflowExecutionResult>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(ExecutionStatus.Completed);
    }
}
```

### Frontend Testing

#### Component Tests

```typescript
// __tests__/components/WorkflowDesigner.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { WorkflowDesigner } from '@/components/workflow/WorkflowDesigner';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('WorkflowDesigner', () => {
  it('renders workflow designer canvas', () => {
    render(<WorkflowDesigner workflowId="test-id" />, { wrapper: createWrapper() });
    
    expect(screen.getByTestId('react-flow')).toBeInTheDocument();
    expect(screen.getByTestId('node-palette')).toBeInTheDocument();
  });

  it('adds node when dragged from palette', async () => {
    const user = userEvent.setup();
    render(<WorkflowDesigner workflowId="test-id" />, { wrapper: createWrapper() });
    
    const httpNode = screen.getByText('HTTP Request');
    await user.click(httpNode);
    
    await waitFor(() => {
      expect(screen.getByText('HTTP Request Node')).toBeInTheDocument();
    });
  });

  it('connects nodes when edges are created', async () => {
    const user = userEvent.setup();
    render(<WorkflowDesigner workflowId="test-id" />, { wrapper: createWrapper() });
    
    // Add two nodes
    const httpNode = screen.getByText('HTTP Request');
    await user.click(httpNode);
    
    const emailNode = screen.getByText('Email');
    await user.click(emailNode);
    
    // Simulate connecting nodes
    const sourceHandle = screen.getByTestId('source-handle-1');
    const targetHandle = screen.getByTestId('target-handle-2');
    
    fireEvent.mouseDown(sourceHandle);
    fireEvent.mouseMove(targetHandle);
    fireEvent.mouseUp(targetHandle);
    
    await waitFor(() => {
      expect(screen.getByTestId('edge-1-2')).toBeInTheDocument();
    });
  });
});
```

#### Hook Tests

```typescript
// __tests__/hooks/useWorkflow.test.ts
import { renderHook, waitFor } from '@testing-library/react';
import { useWorkflow } from '@/hooks/useWorkflow';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as workflowApi from '@/lib/api/workflow-api';

jest.mock('@/lib/api/workflow-api');

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useWorkflow', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('fetches workflow data on mount', async () => {
    const mockWorkflow = {
      id: 'test-id',
      name: 'Test Workflow',
      nodes: [],
      edges: []
    };

    (workflowApi.getWorkflow as jest.Mock).mockResolvedValue(mockWorkflow);

    const { result } = renderHook(() => useWorkflow('test-id'), { wrapper: createWrapper() });

    await waitFor(() => {
      expect(result.current.workflow).toEqual(mockWorkflow);
      expect(result.current.isLoading).toBe(false);
    });
  });

  it('updates workflow when updateWorkflow is called', async () => {
    const mockWorkflow = {
      id: 'test-id',
      name: 'Test Workflow',
      nodes: [],
      edges: []
    };

    (workflowApi.getWorkflow as jest.Mock).mockResolvedValue(mockWorkflow);
    (workflowApi.updateWorkflow as jest.Mock).mockResolvedValue({ ...mockWorkflow, name: 'Updated Workflow' });

    const { result } = renderHook(() => useWorkflow('test-id'), { wrapper: createWrapper() });

    await waitFor(() => {
      expect(result.current.workflow).toEqual(mockWorkflow);
    });

    await result.current.updateWorkflow({ name: 'Updated Workflow' });

    await waitFor(() => {
      expect(result.current.workflow?.name).toBe('Updated Workflow');
      expect(result.current.isDirty).toBe(false);
    });
  });
});
```

### End-to-End Testing

#### Playwright Tests

```typescript
// e2e/workflow-creation.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Workflow Creation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password');
    await page.click('[data-testid="login-button"]');
    await expect(page).toHaveURL('/dashboard');
  });

  test('creates a new workflow', async ({ page }) => {
    // Navigate to workflow creation
    await page.click('[data-testid="create-workflow"]');
    await expect(page).toHaveURL('/workflows/new');

    // Fill workflow details
    await page.fill('[data-testid="workflow-name"]', 'E2E Test Workflow');
    await page.fill('[data-testid="workflow-description"]', 'Test workflow created by E2E test');

    // Add HTTP Request node
    await page.dragAndDrop(
      '[data-testid="node-palette-http"]', 
      '[data-testid="workflow-canvas"]'
    );

    // Configure the node
    await page.click('[data-testid="node-http-1"]');
    await page.fill('[data-testid="node-config-url"]', 'https://api.example.com/test');
    await page.selectOption('[data-testid="node-config-method"]', 'GET');

    // Add Email node
    await page.dragAndDrop(
      '[data-testid="node-palette-email"]', 
      '[data-testid="workflow-canvas"]'
    );

    // Connect nodes
    await page.hover('[data-testid="node-http-1"]');
    await page.dragAndDrop(
      '[data-testid="handle-source-http-1"]',
      '[data-testid="handle-target-email-2"]'
    );

    // Save workflow
    await page.click('[data-testid="save-workflow"]');
    await expect(page.locator('[data-testid="save-success"]')).toBeVisible();

    // Execute workflow
    await page.click('[data-testid="execute-workflow"]');
    await expect(page.locator('[data-testid="execution-started"]')).toBeVisible();

    // Wait for execution to complete
    await expect(page.locator('[data-testid="execution-completed"]')).toBeVisible({ timeout: 30000 });
  });

  test('validates workflow before execution', async ({ page }) => {
    await page.click('[data-testid="create-workflow"]');
    
    // Create invalid workflow (node without configuration)
    await page.dragAndDrop(
      '[data-testid="node-palette-http"]', 
      '[data-testid="workflow-canvas"]'
    );

    // Try to execute without configuring
    await page.click('[data-testid="execute-workflow"]');
    
    // Should show validation errors
    await expect(page.locator('[data-testid="validation-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="validation-error"]')).toContainText('URL is required');
  });
});
```

### Performance Testing

#### Load Testing with k6

```javascript
// k6/workflow-execution-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests under 500ms
    http_req_failed: ['rate<0.1'],    // Error rate under 10%
    errors: ['rate<0.1'],             // Custom error rate under 10%
  },
};

const BASE_URL = 'http://localhost:5001';
let authToken;

export function setup() {
  // Login and get auth token
  const loginRes = http.post(`${BASE_URL}/api/auth/login`, {
    email: 'loadtest@example.com',
    password: 'LoadTest123!'
  });
  
  check(loginRes, {
    'login successful': (r) => r.status === 200,
  });
  
  return { token: loginRes.json('token') };
}

export default function(data) {
  const headers = {
    'Authorization': `Bearer ${data.token}`,
    'Content-Type': 'application/json',
  };

  // Create workflow
  const createWorkflowPayload = {
    name: `Load Test Workflow ${__VU}-${__ITER}`,
    description: 'Load test workflow',
    definition: {
      nodes: [
        {
          id: 'node1',
          type: 'HttpRequest',
          data: {
            url: 'https://httpbin.org/get',
            method: 'GET'
          }
        }
      ],
      edges: []
    }
  };

  const createRes = http.post(
    `${BASE_URL}/api/workflows`, 
    JSON.stringify(createWorkflowPayload), 
    { headers }
  );

  const createSuccess = check(createRes, {
    'workflow created': (r) => r.status === 201,
  });

  if (!createSuccess) {
    errorRate.add(1);
    return;
  }

  const workflowId = createRes.json('id');

  // Execute workflow
  const executePayload = {
    inputData: { testValue: `load-test-${__VU}-${__ITER}` }
  };

  const executeRes = http.post(
    `${BASE_URL}/api/workflows/${workflowId}/execute`,
    JSON.stringify(executePayload),
    { headers }
  );

  const executeSuccess = check(executeRes, {
    'workflow executed': (r) => r.status === 200,
    'execution has id': (r) => r.json('executionId') !== undefined,
  });

  if (!executeSuccess) {
    errorRate.add(1);
  }

  // Wait for execution to complete
  let executionCompleted = false;
  let attempts = 0;
  const maxAttempts = 30;

  while (!executionCompleted && attempts < maxAttempts) {
    sleep(1);
    
    const statusRes = http.get(
      `${BASE_URL}/api/executions/${executeRes.json('executionId')}`,
      { headers }
    );

    if (statusRes.status === 200) {
      const status = statusRes.json('status');
      executionCompleted = status === 'Completed' || status === 'Failed';
    }
    
    attempts++;
  }

  check(null, {
    'execution completed in time': () => executionCompleted,
  });

  if (!executionCompleted) {
    errorRate.add(1);
  }

  sleep(1);
}

export function teardown(data) {
  // Cleanup if needed
  console.log('Load test completed');
}
```

### Test Automation

#### GitHub Actions Test Workflow

```yaml
# .github/workflows/test.yml
name: Test Suite

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: test_password
          POSTGRES_USER: test_user
          POSTGRES_DB: workflow_test
        ports: [5432:5432]
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

      redis:
        image: redis:7
        ports: [6379:6379]
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run unit tests
      run: dotnet test --no-build --configuration Release --logger trx --collect:"XPlat Code Coverage"
      env:
        DATABASE_CONNECTION_STRING: "Host=localhost;Database=workflow_test;Username=test_user;Password=test_password"
        REDIS_CONNECTION_STRING: "localhost:6379"
    
    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results
        path: '**/TestResults/**/*'
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: '**/coverage.cobertura.xml'

  frontend-tests:
    runs-on: ubuntu-latest
    
    defaults:
      run:
        working-directory: ./workflow-platform-frontend
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: workflow-platform-frontend/package-lock.json
    
    - name: Install dependencies
      run: npm ci
    
    - name: Lint
      run: npm run lint
    
    - name: Type check
      run: npm run type-check
    
    - name: Run tests
      run: npm run test:coverage
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: workflow-platform-frontend/coverage/lcov.info

  e2e-tests:
    runs-on: ubuntu-latest
    needs: [backend-tests, frontend-tests]
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
    
    - name: Start services
      run: |
        docker-compose up -d postgres redis rabbitmq
        sleep 10
    
    - name: Start backend
      run: |
        cd WorkflowPlatform.Api
        dotnet run &
        sleep 30
      env:
        ASPNETCORE_ENVIRONMENT: Development
        DATABASE_CONNECTION_STRING: "Host=localhost;Database=workflow_dev;Username=workflow_user;Password=dev_password"
    
    - name: Start frontend
      run: |
        cd workflow-platform-frontend
        npm ci
        npm run build
        npm start &
        sleep 30
    
    - name: Install Playwright
      run: |
        cd workflow-platform-frontend
        npx playwright install
    
    - name: Run E2E tests
      run: |
        cd workflow-platform-frontend
        npx playwright test
    
    - name: Upload E2E results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: e2e-results
        path: workflow-platform-frontend/test-results/

  performance-tests:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    needs: [backend-tests, frontend-tests]
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup k6
      run: |
        sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
        echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
        sudo apt-get update
        sudo apt-get install k6
    
    - name: Start services for load testing
      run: |
        docker-compose -f docker-compose.loadtest.yml up -d
        sleep 60
    
    - name: Run performance tests
      run: k6 run k6/workflow-execution-load-test.js
    
    - name: Upload performance results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: performance-results
        path: k6-results/
```

## 🤝 Contributing

### Getting Started

1. **Fork the Repository**

   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/your-username/workflow-platform.git
   cd workflow-platform
   ```

2. **Set Up Development Environment**

   ```bash
   # Install prerequisites
   # Follow the Development Setup section above
   
   # Create feature branch
   git checkout -b feature/your-feature-name
   ```

3. **Make Changes**
   - Follow coding standards and guidelines
   - Write tests for new functionality
   - Update documentation as needed

4. **Submit Pull Request**
   - Ensure all tests pass
   - Follow PR template
   - Request review from maintainers

### Coding Standards

#### Backend Coding Standards

```csharp
// Use explicit types for clarity
public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(
    Guid workflowId, 
    CancellationToken cancellationToken = default)
{
    // Method implementation
}

// Prefer readonly fields
private readonly ILogger<WorkflowService> _logger;

// Use meaningful variable names
var activeWorkflowInstances = await GetActiveWorkflowInstancesAsync();

// Async methods should have Async suffix
public async Task<bool> ValidateWorkflowAsync(WorkflowDefinition definition)
{
    // Implementation
}
```

#### Frontend (TypeScript)

```typescript
// Use explicit types
interface WorkflowNode {
  id: string;
  type: string;
  data: Record<string, unknown>;
  position: { x: number; y: number };
}

// Prefer const assertions for immutable data
const NODE_TYPES = [
  'HttpRequest',
  'DatabaseQuery',
  'EmailNotification'
] as const;

// Use descriptive function names
const handleNodeSelection = useCallback((nodeId: string) => {
  // Implementation
}, []);

// Component props should be typed
interface WorkflowDesignerProps {
  workflowId: string;
  readonly?: boolean;
  onSave?: (workflow: WorkflowDefinition) => void;
}
```

### Pull Request Process

1. **Before Submitting**
   - [ ] All tests pass locally
   - [ ] Code follows style guidelines
   - [ ] Documentation updated
   - [ ] No breaking changes (or clearly documented)

2. **PR Requirements**
   - [ ] Descriptive title and description
   - [ ] Linked to relevant issues
   - [ ] Appropriate labels applied
   - [ ] Reviewers assigned

3. **Review Process**
   - Minimum 2 approvals required
   - All conversations resolved
   - CI/CD checks passing
   - No merge conflicts

### Code Review Guidelines

#### For Authors

- Keep PRs focused and reasonably sized
- Write clear commit messages
- Respond to feedback promptly
- Update PR based on review comments

#### For Reviewers

- Review for correctness, performance, and maintainability
- Provide constructive feedback
- Approve when confident in changes
- Focus on important issues, not style preferences

### Issue Reporting

#### Bug Reports

```markdown
**Bug Description**
A clear description of the bug.

**Steps to Reproduce**
1. Go to '...'
2. Click on '....'
3. See error

**Expected Behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment**
- OS: [e.g. Windows 10]
- Browser: [e.g. Chrome 91]
- Version: [e.g. 1.0.0]

**Additional Context**
Any other context about the problem.
```

#### Feature Requests

```markdown
**Feature Description**
A clear description of the feature you'd like to see.

**Use Case**
Describe the problem this feature would solve.

**Proposed Solution**
Your ideas for how this could be implemented.

**Alternatives Considered**
Other approaches you've considered.

**Additional Context**
Any other context or screenshots.
```

### Community Guidelines

- Be respectful and inclusive
- Follow the code of conduct
- Help others learn and grow
- Share knowledge and best practices
- Contribute documentation improvements

## 🔧 Troubleshooting

### Common Issues

#### Development Issues

##### Issue: Database connection fails

```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Check connection string
dotnet user-secrets list

# Reset database
dotnet ef database drop
dotnet ef database update
```

##### Issue: Frontend build fails

```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm cache clean --force
npm install

# Check Node.js version
node --version  # Should be 18+
```

##### Issue: SignalR connection fails

```bash
# Check CORS configuration
# Ensure frontend URL is in CORS_ORIGINS

# Check authentication
# Verify JWT token is being sent correctly

# Check network
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5001/api/health
```

#### Production Issues

##### Issue: Kubernetes pods not starting

```bash
# Check pod status
kubectl get pods -n workflow-platform

# Check pod logs
kubectl logs -f deployment/workflow-api -n workflow-platform

# Check events
kubectl get events -n workflow-platform --sort-by='.lastTimestamp'

# Check resource limits
kubectl describe pod POD_NAME -n workflow-platform
```

##### Issue: High memory usage

```bash
# Check memory metrics
kubectl top pods -n workflow-platform

# Scale down temporarily
kubectl scale deployment workflow-api --replicas=1 -n workflow-platform

# Check for memory leaks in logs
kubectl logs deployment/workflow-api -n workflow-platform | grep -i "memory\|gc"
```

##### Issue: Database performance issues

```sql
-- Check active connections
SELECT count(*) FROM pg_stat_activity WHERE state = 'active';

-- Check slow queries
SELECT query, mean_exec_time, calls, total_exec_time 
FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;

-- Check index usage
SELECT schemaname, tablename, attname, n_distinct, correlation 
FROM pg_stats 
WHERE tablename = 'workflow_instances';
```

### Performance Troubleshooting

#### API Response Times

```bash
# Check metrics endpoint
curl http://localhost:5001/metrics | grep http_request_duration

# Monitor with curl
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:5001/api/workflows

# curl-format.txt
     time_namelookup:  %{time_namelookup}\n
        time_connect:  %{time_connect}\n
     time_appconnect:  %{time_appconnect}\n
    time_pretransfer:  %{time_pretransfer}\n
       time_redirect:  %{time_redirect}\n
  time_starttransfer:  %{time_starttransfer}\n
                     ----------\n
          time_total:  %{time_total}\n
```

#### Database Performance

```sql
-- Enable query logging temporarily
ALTER SYSTEM SET log_statement = 'all';
ALTER SYSTEM SET log_min_duration_statement = 100;
SELECT pg_reload_conf();

-- Check for locks
SELECT 
    blocked_locks.pid AS blocked_pid,
    blocked_activity.usename AS blocked_user,
    blocking_locks.pid AS blocking_pid,
    blocking_activity.usename AS blocking_user,
    blocked_activity.query AS blocked_statement,
    blocking_activity.query AS current_statement_in_blocking_process
FROM pg_catalog.pg_locks blocked_locks
JOIN pg_catalog.pg_stat_activity blocked_activity ON blocked_activity.pid = blocked_locks.pid
JOIN pg_catalog.pg_locks blocking_locks 
    ON blocking_locks.locktype = blocked_locks.locktype
    AND blocking_locks.database IS NOT DISTINCT FROM blocked_locks.database
    AND blocking_locks.relation IS NOT DISTINCT FROM blocked_locks.relation
    AND blocking_locks.page IS NOT DISTINCT FROM blocked_locks.page
    AND blocking_locks.tuple IS NOT DISTINCT FROM blocked_locks.tuple
    AND blocking_locks.virtualxid IS NOT DISTINCT FROM blocked_locks.virtualxid
    AND blocking_locks.transactionid IS NOT DISTINCT FROM blocked_locks.transactionid
    AND blocking_locks.classid IS NOT DISTINCT FROM blocked_locks.classid
    AND blocking_locks.objid IS NOT DISTINCT FROM blocked_locks.objid
    AND blocking_locks.objsubid IS NOT DISTINCT FROM blocked_locks.objsubid
    AND blocking_locks.pid != blocked_locks.pid
JOIN pg_catalog.pg_stat_activity blocking_activity ON blocking_activity.pid = blocking_locks.pid
WHERE NOT blocked_locks.granted;
```

### Monitoring and Alerting

#### Health Check Endpoints

```bash
# API health
curl http://localhost:5001/health

# Detailed health
curl http://localhost:5001/health/ready

# Metrics
curl http://localhost:5001/metrics
```

#### Log Analysis

```bash
# Search for errors in Docker logs
docker logs workflow-api 2>&1 | grep -i error

# Search for specific correlation ID
docker logs workflow-api 2>&1 | grep "CorrelationId: 12345"

# Monitor real-time logs
docker logs -f workflow-api
```

### Recovery Procedures

#### Database Recovery

```bash
# Backup current database
kubectl exec -it postgres-pod -- pg_dump workflow_platform > backup.sql

# Restore from backup
kubectl exec -i postgres-pod -- psql workflow_platform < backup.sql

# Check database integrity
kubectl exec -it postgres-pod -- psql workflow_platform -c "SELECT pg_size_pretty(pg_database_size('workflow_platform'));"
```

#### Application Recovery

```bash
# Rolling restart
kubectl rollout restart deployment/workflow-api -n workflow-platform

# Scale to zero and back
kubectl scale deployment workflow-api --replicas=0 -n workflow-platform
kubectl scale deployment workflow-api --replicas=3 -n workflow-platform

# Check rollout status
kubectl rollout status deployment/workflow-api -n workflow-platform
```

## ❓ FAQ

### General Questions

**Q: What is the minimum hardware requirement?**
A: For development: 8GB RAM, 4 CPU cores. For production: 16GB RAM, 8 CPU cores per node, with at least 3 nodes for high availability.

**Q: Can I use MySQL instead of PostgreSQL?**
A: PostgreSQL is recommended for its advanced JSON support and reliability. MySQL support is not officially maintained but could be added via Entity Framework Core.

**Q: How do I create custom node types?**
A: Implement the `INodeExecutionStrategy` interface and register it in the DI container. See the [Custom Nodes](#creating-custom-nodes) section.

**Q: Is there a workflow size limit?**
A: Soft limit of 500 nodes per workflow for optimal performance. Hard limit of 10MB for workflow definition JSON.

### Technical Questions

**Q: How are secrets handled in workflows?**
A: Secrets are encrypted using ASP.NET Core Data Protection and stored separately from workflow definitions. They're injected at runtime.

**Q: Can workflows call other workflows?**
A: Yes, use the "Subworkflow" node type to execute other workflows and pass data between them.

**Q: How is horizontal scaling achieved?**
A: The application is stateless with external state storage (PostgreSQL, Redis). Use Kubernetes HPA for auto-scaling based on CPU/memory metrics.

**Q: What's the maximum execution time for a workflow?**
A: Default timeout is 30 minutes, configurable up to 24 hours. Long-running workflows should use checkpointing.

### Deployment Questions

**Q: Can I deploy without Kubernetes?**
A: Yes, use Docker Compose for single-node deployments or Docker Swarm for multi-node setups.

**Q: How do I backup the system?**
A: Backup PostgreSQL database, Redis snapshots, and workflow definition files. See disaster recovery documentation.

**Q: Is multi-tenancy supported?**
A: Yes, through tenant isolation at the database level with row-level security policies.

**Q: How do I migrate from version X to Y?**
A: Follow the migration guide in the release notes. Database migrations are handled automatically by Entity Framework.

## 🗺 Roadmap

### Version 1.1 (Q1 2025)

- [ ] **Advanced Node Types**
  - AI/ML integration nodes (TensorFlow, PyTorch)
  - Advanced data transformation nodes
  - Message queue nodes (Kafka, Azure Service Bus)
  
- [ ] **Enhanced UI**
  - Dark mode support
  - Mobile-responsive design
  - Accessibility improvements (WCAG 2.1 AAA)

- [ ] **Performance Improvements**
  - GraphQL API endpoints
  - Enhanced caching strategies
  - Database query optimization

### Version 1.2 (Q2 2025)

- [ ] **Enterprise Features**
  - Multi-tenancy with complete data isolation
  - Advanced RBAC with custom roles
  - Comprehensive audit logging
  
- [ ] **Integration Marketplace**
  - Popular SaaS platform connectors (Salesforce, ServiceNow, etc.)
  - Community-contributed node types
  - Workflow template sharing

- [ ] **Advanced Monitoring**
  - Business process monitoring
  - SLA tracking and alerting
  - Cost optimization recommendations

### Version 1.3 (Q3 2025)

- [ ] **AI-Powered Features**
  - Workflow optimization recommendations
  - Predictive failure detection
  - Auto-scaling based on pattern analysis
  
- [ ] **Developer Experience**
  - Visual Studio Code extension
  - CLI tools for workflow management
  - Local development environment

- [ ] **Advanced Scheduling**
  - Cron-based scheduling
  - Event-driven triggers
  - Workflow orchestration patterns

### Version 2.0 (Q4 2025)

- [ ] **Multi-Region Support**
  - Global workflow distribution
  - Edge computing integration
  - Cross-region data replication
  
- [ ] **Advanced Analytics**
  - Business intelligence dashboards
  - Process mining capabilities
  - Performance analytics and insights

- [ ] **Ecosystem Integration**
  - OpenTelemetry support
  - Service mesh integration
  - Cloud-native enhancements

### Long-term Vision

- **Industry-Specific Solutions**: Healthcare, finance, manufacturing workflow templates
- **Low-Code Platform**: Visual programming interface for business users
- **AI Workflow Assistant**: Natural language workflow creation
- **Real-time Collaboration**: Google Docs-style collaborative editing
- **Blockchain Integration**: Immutable workflow audit trails

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### MIT License Summary

```text
Copyright (c) 2024 Workflow Platform Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🆘 Support

### Getting Help

- **📖 Documentation**: [docs.workflow-platform.com](https://docs.workflow-platform.com)
- **💬 Community**: [GitHub Discussions](https://github.com/company/workflow-platform/discussions)
- **🐛 Bug Reports**: [GitHub Issues](https://github.com/company/workflow-platform/issues)
- **💡 Feature Requests**: [GitHub Issues](https://github.com/company/workflow-platform/issues/new?template=feature_request.md)

### Commercial Support

For enterprise support, training, and consulting services:

- **Email**: <support@workflow-platform.com>
- **Website**: [workflow-platform.com/support](https://workflow-platform.com/support)
- **Phone**: +1 (555) 123-4567

### Community Resources

- **Stack Overflow**: Tag questions with `workflow-platform`
- **Reddit**: [r/WorkflowPlatform](https://reddit.com/r/WorkflowPlatform)
- **Discord**: [Workflow Platform Community](https://discord.gg/workflow-platform)
- **Twitter**: [@WorkflowPlatform](https://twitter.com/WorkflowPlatform)

### Contributing

We welcome contributions! Please see our [Contributing Guide](#contributing) for details on how to get started.

---

**⭐ Star this repository if you find it helpful!**

[![GitHub stars](https://img.shields.io/github/stars/company/workflow-platform.svg?style=social&label=Star&maxAge=2592000)](https://github.com/company/workflow-platform/stargazers/)

Made with ❤️ by the Workflow Platform team
