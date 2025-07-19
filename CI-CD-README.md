# CI/CD Pipeline Foundation

This document describes the CI/CD pipeline infrastructure implemented for the Workflow Orchestration Platform, following enterprise-grade DevOps practices and security standards.

## 🏗️ Pipeline Architecture

```bash
Main Pipeline → Backend Pipeline → Security Scan → Build & Push → Deploy
            → Frontend Pipeline → Test Suite → Quality Gates → Staging
```

### Pipeline Components

1. **Main Workflow** (`main.yml`) - Orchestrates the entire CI/CD process
2. **Backend Pipeline** (`backend.yml`) - .NET API build, test, and deployment
3. **Frontend Pipeline** (`frontend.yml`) - Next.js build, test, and deployment  
4. **Deployment Pipeline** (`deploy.yml`) - Kubernetes deployment orchestration

## 🚀 Features

### Security & Quality

- ✅ **CodeQL Security Scanning** - Automated vulnerability detection
- ✅ **Trivy Container Scanning** - Docker image security analysis
- ✅ **SonarCloud Integration** - Code quality and security gates
- ✅ **SBOM Generation** - Software Bill of Materials for compliance
- ✅ **Image Signing** - Cosign-based container image signing

### Testing & Validation

- ✅ **Unit Tests** - Comprehensive test coverage with reporting
- ✅ **Integration Tests** - Database and service integration validation
- ✅ **E2E Tests** - End-to-end user workflow testing
- ✅ **Performance Tests** - API response time and load testing
- ✅ **Lighthouse Audits** - Frontend performance and accessibility

### Deployment & Operations

- ✅ **Multi-stage Docker Builds** - Optimized container images
- ✅ **Kubernetes Deployment** - Helm-based production deployment
- ✅ **Zero-downtime Deployment** - Rolling updates with health checks
- ✅ **Environment Promotion** - Staging → Production pipeline
- ✅ **Automated Rollbacks** - Failure detection and recovery

## 📁 Repository Structure

```bash
.github/workflows/
├── main.yml           # Main CI/CD orchestration
├── backend.yml        # .NET backend pipeline
├── frontend.yml       # Next.js frontend pipeline
└── deploy.yml         # Kubernetes deployment

docker/
├── postgres/          # PostgreSQL configuration
├── redis/            # Redis configuration
├── rabbitmq/         # RabbitMQ configuration
├── prometheus/       # Monitoring configuration
└── grafana/          # Dashboards and datasources

helm/
└── workflow-platform/ # Kubernetes Helm chart
    ├── Chart.yaml
    ├── values.yaml
    └── templates/

src/WorkflowPlatform.API/
└── Dockerfile        # Multi-stage API container

docker-compose.yml     # Development environment
docker-compose.prod.yml # Production overrides
sonar-project.properties # SonarCloud configuration
coverlet.runsettings  # Code coverage settings
```

## 🔧 Pipeline Configuration

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `REGISTRY` | Container registry URL | `ghcr.io` |
| `IMAGE_NAME` | Base image name | `${{ github.repository }}` |
| `DOTNET_VERSION` | .NET SDK version | `8.0.x` |
| `NODE_VERSION` | Node.js version | `18.x` |

### GitHub Secrets Required

#### Production Deployment

- `KUBECONFIG` - Kubernetes cluster configuration
- `DATABASE_CONNECTION_STRING` - Production database connection
- `REDIS_CONNECTION_STRING` - Redis connection string
- `RABBITMQ_CONNECTION_STRING` - RabbitMQ connection string
- `JWT_SECRET` - JWT signing secret
- `JWT_ISSUER` - JWT issuer identifier
- `JWT_AUDIENCE` - JWT audience identifier

#### Code Quality & Security

- `SONAR_TOKEN` - SonarCloud authentication token
- `GITHUB_TOKEN` - GitHub API token (automatically provided)

#### Monitoring

- `GRAFANA_PASSWORD` - Grafana admin password

## 🏃‍♂️ Development Setup

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Node.js 18+
- Git

### Quick Start

#### Windows

```powershell
.\setup-dev.bat
```

#### Linux/macOS

```bash
chmod +x setup-dev.sh
./setup-dev.sh
```

### Manual Setup

1. **Clone and build**

   ```bash
   git clone https://github.com/ribatshepo/WorkFlowOchestrator.git
   cd WorkFlowOchestrator
   dotnet restore WorkflowPlatform.sln
   dotnet build WorkflowPlatform.sln
   ```

2. **Start infrastructure services**

   ```bash
   docker-compose up -d postgres redis rabbitmq
   ```

3. **Configure secrets**

   ```bash
   cd src/WorkflowPlatform.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=WorkflowPlatformDev;Username=workflow_user;Password=dev_password_2024!;Port=5432"
   # ... additional secrets
   ```

4. **Start the API**

   ```bash
   dotnet run
   ```

## 🧪 Testing Strategy

### Backend Testing

```bash
# Unit tests
dotnet test --configuration Release --collect:"XPlat Code Coverage"

# Integration tests with test database
ConnectionStrings__DefaultConnection="Host=localhost;Database=workflow_test;..." \
dotnet test tests/**/*Integration.Tests.csproj
```

### Frontend Testing

```bash
cd workflow-platform-frontend
npm install
npm run test:coverage          # Unit tests
npm run test:e2e              # End-to-end tests
npm run lighthouse            # Performance audit
```

## 📦 Container Registry Strategy

### Image Tagging

- `latest` - Latest main branch build
- `{branch}-{sha}` - Branch-specific builds
- `{branch}-{sha}-{timestamp}` - Unique timestamped builds
- `{pr-number}` - Pull request builds

### Multi-platform Support

- `linux/amd64` - x86_64 architecture
- `linux/arm64` - ARM64 architecture

### Security Features

- Non-root container execution
- Minimal base images (Alpine Linux)
- Security updates applied during build
- Image signing with Cosign
- SBOM generation for compliance

## 🚀 Deployment Process

### Staging Deployment

Triggered on pushes to `develop` branch:

1. Build and test validation
2. Container image creation
3. Deployment to staging namespace
4. Smoke test execution
5. Integration test validation

### Production Release Process

Triggered on pushes to `main` branch:

1. All staging requirements passed
2. Security and quality gates cleared
3. Deployment to production namespace
4. Blue-green deployment strategy
5. Health check validation
6. Automated rollback on failure

### Kubernetes Resources

- **Namespace**: `workflow-platform-{environment}`
- **Ingress**: NGINX with SSL termination
- **Secrets**: External secret management
- **Monitoring**: Prometheus/Grafana integration
- **Autoscaling**: HPA based on CPU/memory

## 📊 Monitoring & Observability

### Metrics Collection

- **Prometheus** - Application and infrastructure metrics
- **Grafana** - Visualization dashboards
- **Jaeger** - Distributed tracing
- **Loki** - Centralized logging (optional)

### Health Checks

- `/health` - Basic application health
- `/ready` - Readiness probe
- `/metrics` - Prometheus metrics endpoint

### Alerting

- High error rates
- Performance degradation
- Resource exhaustion
- Security incidents

## 🔒 Security Best Practices

### Container Security

- Non-root user execution
- Read-only root filesystem
- Security context constraints
- Network policies
- Image vulnerability scanning

### Secret Management

- External secret stores (Azure Key Vault, AWS Secrets Manager)
- Kubernetes secrets
- No hardcoded credentials
- Regular secret rotation

### Network Security

- TLS encryption in transit
- Network segmentation
- Ingress traffic filtering
- Service mesh integration ready

## 🏥 Disaster Recovery

### Backup Strategy

- Database automated backups
- Configuration backups
- Container image registry replication

### Recovery Procedures

- Automated rollback on deployment failure
- Point-in-time database recovery
- Cross-region deployment capability

## 📈 Performance Optimization

### Build Performance

- Docker layer caching
- Multi-stage builds
- Parallel job execution
- Artifact caching

### Runtime Performance

- Resource limits and requests
- Horizontal pod autoscaling
- Connection pooling
- CDN integration ready

## 🔧 Troubleshooting

### Common Issues

#### Pipeline Failures

```bash
# View pipeline logs
gh run view [run-id] --log

# Re-run failed jobs
gh run rerun [run-id] --failed
```

#### Local Development Issues

```bash
# Reset development environment
docker-compose down -v
./setup-dev.sh

# View service logs
docker-compose logs -f [service-name]

# Check service health
docker-compose ps
```

#### Deployment Issues

```bash
# Check Kubernetes resources
kubectl get all -n workflow-platform-staging

# View pod logs
kubectl logs -f deployment/workflow-platform-staging-api

# Describe problematic resources
kubectl describe pod [pod-name] -n workflow-platform-staging
```

## 📚 References

- [Implementation Plan](../Docs/ImplementationPlan.md) - Detailed project roadmap
- [Technical Design](../Docs/TechnicalDesign.md) - Architecture specifications  
- [Security Guide](../Docs/SECURITY-CONFIG-GUIDE.md) - Security configuration
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Helm Documentation](https://helm.sh/docs/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests locally
5. Submit a pull request

All pull requests trigger the full CI/CD pipeline for validation.

---

**Epic Status**: ✅ **Completed** - WOP-E003.1 CI/CD Pipeline Foundation

**Next Steps**: Deploy to staging environment and validate pipeline functionality
