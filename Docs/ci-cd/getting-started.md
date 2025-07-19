# Getting Started with CI/CD Pipeline

## Prerequisites

Before setting up the CI/CD pipeline, ensure you have:

### Required Access

- **GitHub Repository**: Admin access to the WorkflowOchestrator repository
- **Kubernetes Cluster**: Access to target deployment cluster(s)
- **Container Registry**: GitHub Container Registry access (automatic with repository access)

### Required Tools (for local development)

- **Git**: Version 2.30 or higher
- **Docker**: Version 20.10 or higher
- **kubectl**: Version 1.25 or higher
- **.NET SDK**: Version 8.0 or higher
- **Node.js**: Version 18.x or higher

### Optional Tools

- **Helm**: Version 3.10 or higher (for manual deployments)
- **SonarCloud CLI**: For local code quality analysis

## Initial Setup

### Step 1: Repository Configuration

1. **Enable GitHub Actions**

   ```bash
   # Navigate to your repository settings
   # Go to Actions > General
   # Ensure "Allow all actions and reusable workflows" is selected
   ```

2. **Configure Branch Protection**

   ```bash
   # Set up branch protection rules for main and develop branches
   # Require pull request reviews
   # Require status checks to pass before merging
   # Include administrators in restrictions
   ```

### Step 2: Environment Secrets

Configure the following secrets in your GitHub repository settings:

#### Required Secrets

```bash
# Repository Settings > Secrets and variables > Actions

# SonarCloud Integration (Optional but Recommended)
SONAR_TOKEN=your_sonarcloud_project_token

# Kubernetes Deployment
KUBECONFIG_STAGING=base64_encoded_kubeconfig_for_staging
KUBECONFIG_PRODUCTION=base64_encoded_kubeconfig_for_production

# Database Credentials
DB_CONNECTION_STRING_STAGING=postgresql://user:pass@host:5432/dbname
DB_CONNECTION_STRING_PRODUCTION=postgresql://user:pass@host:5432/dbname

# Additional Environment Variables
JWT_SECRET_STAGING=your_staging_jwt_secret_minimum_256_bits
JWT_SECRET_PRODUCTION=your_production_jwt_secret_minimum_256_bits
```

#### Optional Secrets

```bash
# Notification Services
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx
TEAMS_WEBHOOK_URL=https://your-team.webhook.office.com/xxx

# External Services
SENDGRID_API_KEY=your_sendgrid_api_key
AZURE_CLIENT_SECRET=your_azure_client_secret
```

### Step 3: Local Development Environment

1. **Clone the Repository**

   ```bash
   git clone https://github.com/ribatshepo/WorkFlowOchestrator.git
   cd WorkFlowOchestrator
   ```

2. **Setup Development Environment**

   ```bash
   # Copy environment template
   cp .env.template .env
   
   # Edit .env with your local development values
   nano .env
   ```

3. **Start Development Services**

   ```bash
   # Start all services using docker-compose
   docker-compose up -d
   
   # Verify services are running
   docker-compose ps
   ```

4. **Configure User Secrets (for .NET development)**

   ```bash
   cd src/WorkflowPlatform.API
   
   # Initialize user secrets
   dotnet user-secrets init
   
   # Add development secrets
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your_local_db_connection"
   dotnet user-secrets set "Jwt:Secret" "your_local_jwt_secret_minimum_256_bits"
   dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform.Dev"
   dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform.Users"
   ```

## Your First Pipeline Run

### Automated Trigger (Recommended)

1. **Create a Feature Branch**

   ```bash
   # Create and switch to a new feature branch
   git checkout -b feature/setup-pipeline
   
   # Make a small change (e.g., update README)
   echo "Pipeline setup complete" >> README.md
   
   # Commit and push
   git add README.md
   git commit -m "feat: setup CI/CD pipeline"
   git push origin feature/setup-pipeline
   ```

2. **Create Pull Request**
   - Go to GitHub and create a pull request from your feature branch to `develop`
   - The pipeline will automatically trigger and run all checks
   - Review the Actions tab to monitor progress

3. **Monitor Pipeline Execution**

   ```bash
   # GitHub Actions will show:
   # ✅ Change Detection
   # ✅ Backend Pipeline (if backend changes detected)
   # ✅ Frontend Pipeline (if frontend changes detected)
   # ✅ Quality Gates
   # ✅ Security Scans
   ```

### Manual Trigger

1. **Navigate to Actions Tab**
   - Go to your repository's Actions tab
   - Select "Main CI/CD Pipeline"
   - Click "Run workflow"

2. **Configure Manual Run**

   ```yaml
   # Options available:
   Branch: main                    # Select branch to run against
   Force backend: true            # Force backend pipeline regardless of changes
   Force frontend: false          # Force frontend pipeline regardless of changes
   ```

3. **Monitor Execution**
   - Watch the pipeline execution in real-time
   - Review logs for any issues
   - Check deployment status in your Kubernetes cluster

## Pipeline Stages Explained

### Stage 1: Change Detection

**Duration**: ~30 seconds

The pipeline analyzes changed files to determine which sub-pipelines to execute:

```yaml
# File patterns that trigger backend pipeline:
- 'src/**'                    # Source code changes
- 'tests/**'                  # Test changes
- '*.sln'                     # Solution file changes
- 'Directory.Build.props'     # Build configuration changes

# File patterns that trigger frontend pipeline:
- 'workflow-platform-frontend/**'  # Frontend source changes
- 'docker/frontend/**'             # Frontend Docker changes

# File patterns that trigger infrastructure pipeline:
- 'helm/**'                   # Helm chart changes
- 'k8s/**'                    # Kubernetes manifest changes
- '.github/workflows/**'      # Workflow changes
```

### Stage 2: Parallel Pipeline Execution

**Duration**: 5-8 minutes (parallel)

#### Backend Pipeline

```bash
✅ Code Quality & Security (2-3 minutes)
   ├── Format checking
   ├── Build solution
   ├── Unit tests with coverage
   ├── Integration tests
   └── Security scanning

✅ Container Build & Push (3-4 minutes)
   ├── Multi-stage Docker build
   ├── Vulnerability scanning
   ├── Image signing
   ├── SBOM generation
   └── Push to registry
```

#### Frontend Pipeline

```bash
✅ Code Quality & Testing (2-3 minutes)
   ├── TypeScript type checking
   ├── ESLint linting
   ├── Unit tests with coverage
   ├── Integration tests
   └── Build optimization

✅ Container Build & Push (2-3 minutes)
   ├── Production build
   ├── Docker optimization
   ├── Security scanning
   └── Push to registry
```

### Stage 3: Quality Gates

**Duration**: 1-2 minutes

```bash
✅ SonarCloud Analysis
   ├── Code quality metrics
   ├── Security vulnerability detection
   ├── Technical debt assessment
   └── Coverage validation

✅ Security Baseline Scan
   ├── Container security validation
   ├── Dependency vulnerability check
   └── Configuration security review
```

### Stage 4: Deployment (Conditional)

**Duration**: 3-5 minutes

```bash
✅ Staging Deployment (develop branch)
   ├── Helm chart deployment
   ├── Health check validation
   ├── Smoke tests
   └── Deployment notification

✅ Production Deployment (main branch)
   ├── Staging validation check
   ├── Production Helm deployment
   ├── Health check validation
   ├── Post-deployment verification
   └── Success notification
```

## Verification Steps

### 1. Pipeline Success Verification

```bash
# Check GitHub Actions status
echo "✅ All pipeline stages completed successfully"

# Verify container images were built and pushed
echo "✅ Container images available in GitHub Container Registry"

# Check deployment status (if applicable)
kubectl get deployments -n workflow-platform
kubectl get pods -n workflow-platform
```

### 2. Application Health Check

```bash
# Check API health endpoint
curl -f https://your-staging-url/health

# Verify database connectivity
curl -f https://your-staging-url/health/db

# Check all services status
curl -f https://your-staging-url/health/detailed
```

### 3. Monitoring Verification

```bash
# Access Grafana dashboard
echo "📊 Grafana: https://your-monitoring-url/grafana"

# Check Prometheus metrics
echo "📈 Prometheus: https://your-monitoring-url/prometheus"

# Review application logs
kubectl logs -f deployment/workflow-api -n workflow-platform
```

## Common First-Time Issues

### Issue 1: Missing Secrets

**Symptoms**: Pipeline fails during deployment with authentication errors

**Solution**:

```bash
# Verify all required secrets are configured
# Go to Repository Settings > Secrets and variables > Actions
# Ensure KUBECONFIG_STAGING and KUBECONFIG_PRODUCTION are set
```

### Issue 2: SonarCloud Configuration

**Symptoms**: Quality gates stage fails with SonarCloud authentication error

**Solution**:

```bash
# Option 1: Add SONAR_TOKEN secret (recommended)
# Go to SonarCloud > Your Project > Administration > Security
# Generate new token and add to GitHub secrets

# Option 2: Temporarily disable SonarCloud
# Edit .github/workflows/main.yml
# Comment out the SonarCloud scan step
```

### Issue 3: Docker Build Issues

**Symptoms**: Container build fails with dependency resolution errors

**Solution**:

```bash
# Verify Dockerfile and dependencies
docker build -t test-build .

# Check for missing files or incorrect paths
# Review Docker build context and .dockerignore file
```

### Issue 4: Kubernetes Deployment Failures

**Symptoms**: Deployment stage fails with kubectl errors

**Solution**:

```bash
# Verify kubeconfig is correctly encoded
base64 -d <<< "$KUBECONFIG_STAGING" > /tmp/kubeconfig
kubectl --kubeconfig=/tmp/kubeconfig get nodes

# Check cluster connectivity and permissions
kubectl auth can-i create deployments --namespace workflow-platform
```

## Next Steps

1. **Customize Pipeline**: Review and modify pipeline configuration for your needs
2. **Add Environments**: Set up additional environments (QA, UAT)
3. **Configure Monitoring**: Set up alerts and dashboards
4. **Security Review**: Complete security configuration checklist
5. **Team Training**: Share this documentation with your team

## Getting Help

### Documentation

- [Pipeline Architecture](./pipeline-architecture.md) - Detailed technical overview
- [Configuration Guide](./configuration.md) - Advanced configuration options
- [Security Guidelines](./security.md) - Security best practices
- [Troubleshooting](./troubleshooting.md) - Common issues and solutions

### Support Channels

- **GitHub Issues**: Technical problems and bug reports
- **Team Slack**: `#workflow-platform` - General questions and discussion
- **DevOps Team**: Direct support for pipeline and deployment issues
- **Security Team**: Security-related questions and concerns

---

*Next: [Pipeline Architecture](./pipeline-architecture.md)*
