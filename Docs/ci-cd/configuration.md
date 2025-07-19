# Configuration Guide

## Overview

This guide covers the comprehensive configuration of the CI/CD pipeline, including environment setup, secret management, and customization options.

## Environment Configuration

### GitHub Repository Settings

#### Actions Configuration

Navigate to **Repository Settings > Actions > General**:

```yaml
# Recommended Settings:
Actions permissions: "Allow enterprise, and select non-enterprise, actions and reusable workflows"
Fork pull request workflows: "Require approval for first-time contributors"
Workflow permissions: "Read and write permissions"
```

#### Branch Protection Rules

Configure protection for critical branches:

**Main Branch Protection**:

```yaml
Branch name pattern: main
Protect matching branches: ✅
  Require pull request reviews before merging: ✅
    Required number of reviews: 2
    Dismiss stale reviews: ✅
    Require review from code owners: ✅
  Require status checks before merging: ✅
    Required status checks:
      - "Backend Pipeline"
      - "Frontend Pipeline" 
      - "Quality Gates"
      - "Security Scans"
  Require branches to be up to date: ✅
  Require conversation resolution: ✅
  Include administrators: ✅
```

**Develop Branch Protection**:

```yaml
Branch name pattern: develop
Protect matching branches: ✅
  Require pull request reviews before merging: ✅
    Required number of reviews: 1
  Require status checks before merging: ✅
    Required status checks:
      - "Backend Pipeline"
      - "Frontend Pipeline"
      - "Quality Gates"
```

## Secrets Management

### Required Repository Secrets

Navigate to **Repository Settings > Secrets and variables > Actions**:

#### Infrastructure Secrets

```bash
# Kubernetes Cluster Access
KUBECONFIG_STAGING=<base64-encoded-kubeconfig-file>
KUBECONFIG_PRODUCTION=<base64-encoded-kubeconfig-file>

# To generate base64 encoded kubeconfig:
cat ~/.kube/config | base64 -w 0
```

#### Database Configuration

```bash
# Database Connection Strings
DB_CONNECTION_STRING_STAGING="Host=staging-db.example.com;Database=workflow_platform;Username=app_user;Password=secure_password;SSL Mode=Require;"
DB_CONNECTION_STRING_PRODUCTION="Host=prod-db.example.com;Database=workflow_platform;Username=app_user;Password=secure_password;SSL Mode=Require;"

# Redis Connection
REDIS_CONNECTION_STRING_STAGING="staging-redis.example.com:6379"
REDIS_CONNECTION_STRING_PRODUCTION="prod-redis.example.com:6379"

# Message Queue Configuration
RABBITMQ_CONNECTION_STRING_STAGING="amqp://user:password@staging-rabbitmq.example.com:5672/"
RABBITMQ_CONNECTION_STRING_PRODUCTION="amqp://user:password@prod-rabbitmq.example.com:5672/"
```

#### Application Secrets

```bash
# JWT Configuration
JWT_SECRET_STAGING="<256-bit-secret-key-for-staging>"
JWT_SECRET_PRODUCTION="<256-bit-secret-key-for-production>"
JWT_ISSUER_STAGING="WorkflowPlatform.Staging"
JWT_ISSUER_PRODUCTION="WorkflowPlatform"
JWT_AUDIENCE_STAGING="WorkflowPlatform.Users.Staging"
JWT_AUDIENCE_PRODUCTION="WorkflowPlatform.Users"

# Generate secure JWT secret:
openssl rand -base64 64
```

#### External Services

```bash
# Code Quality
SONAR_TOKEN="<sonarcloud-project-token>"

# Notification Services
SLACK_WEBHOOK_URL="https://hooks.slack.com/services/xxx/xxx/xxx"
TEAMS_WEBHOOK_URL="https://outlook.office.com/webhook/xxx"

# Email Services
SENDGRID_API_KEY="SG.xxx"
SMTP_PASSWORD="<smtp-password>"

# Cloud Provider Credentials
AZURE_CLIENT_ID="<azure-app-registration-client-id>"
AZURE_CLIENT_SECRET="<azure-app-registration-secret>"
AZURE_TENANT_ID="<azure-tenant-id>"

AWS_ACCESS_KEY_ID="<aws-access-key>"
AWS_SECRET_ACCESS_KEY="<aws-secret-key>"
```

### Environment Variables

Navigate to **Repository Settings > Secrets and variables > Actions > Variables**:

```bash
# Application Configuration
APP_NAME="WorkflowPlatform"
APP_VERSION="1.0.0"
DOTNET_VERSION="8.0.x"
NODE_VERSION="18.x"

# Container Registry
REGISTRY="ghcr.io"
IMAGE_NAME_PREFIX="workflowplatform"

# Kubernetes Configuration
K8S_NAMESPACE_STAGING="workflow-platform-staging"
K8S_NAMESPACE_PRODUCTION="workflow-platform"
HELM_CHART_VERSION="1.0.0"

# Monitoring
GRAFANA_URL_STAGING="https://monitoring-staging.example.com"
GRAFANA_URL_PRODUCTION="https://monitoring.example.com"
```

## Workflow Customization

### Modifying Triggers

Edit `.github/workflows/main.yml` to customize when pipelines run:

```yaml
# Current configuration
on:
  push:
    branches: [main, develop, 'feature/*', 'hotfix/*']
  pull_request:
    branches: [main, develop]
  workflow_dispatch:

# Customization examples:
on:
  # Run on all branches
  push:
    branches: ['**']
  
  # Run only on specific paths
  push:
    paths: 
      - 'src/**'
      - '.github/workflows/**'
  
  # Scheduled runs (daily at 2 AM UTC)
  schedule:
    - cron: '0 2 * * *'
  
  # External webhook triggers
  repository_dispatch:
    types: [deploy-staging, deploy-production]
```

### Customizing Change Detection

Modify the change detection filters in `.github/workflows/main.yml`:

```yaml
# Default configuration
filters: |
  backend:
    - 'src/**'
    - 'tests/**'
    - '*.sln'
    - 'Directory.Build.props'
  frontend:
    - 'workflow-platform-frontend/**'
  
# Custom configuration examples
filters: |
  backend:
    - 'src/**'
    - 'tests/**'
    - 'Database/**'          # Include database migrations
    - 'Scripts/**'           # Include deployment scripts
  frontend:
    - 'frontend/**'          # Different frontend directory
    - 'shared/**'            # Shared components
  documentation:
    - 'docs/**'
    - '*.md'
  infrastructure:
    - 'terraform/**'         # Infrastructure as code
    - 'ansible/**'           # Configuration management
```

### Environment-Specific Configuration

Create environment-specific workflow files:

**Staging-Only Workflow** (`.github/workflows/staging-deploy.yml`):

```yaml
name: Staging Deployment

on:
  push:
    branches: [develop]

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - name: Deploy to Staging
        run: |
          helm upgrade --install workflow-platform-staging ./helm/workflow-platform \
            --namespace workflow-platform-staging \
            --values ./helm/workflow-platform/values-staging.yaml
```

**Production-Only Workflow** (`.github/workflows/production-deploy.yml`):

```yaml
name: Production Deployment

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to deploy'
        required: true
        type: string

jobs:
  deploy-production:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Deploy to Production
        run: |
          helm upgrade --install workflow-platform ./helm/workflow-platform \
            --namespace workflow-platform \
            --values ./helm/workflow-platform/values-production.yaml \
            --set image.tag=${{ inputs.version }}
```

## Quality Gates Configuration

### SonarCloud Setup

### Create SonarCloud Project

- Go to [SonarCloud](https://sonarcloud.io)
- Import your GitHub repository
- Note the project key and organization

### Configure sonar-project.properties

```properties
sonar.projectKey=your_organization_WorkFlowOchestrator
sonar.organization=your-organization
sonar.projectName=WorkflowPlatform
sonar.projectVersion=1.0

# Source directories
sonar.sources=src
sonar.tests=tests
sonar.sourceEncoding=UTF-8

# .NET specific settings
sonar.cs.dotcover.reportsPaths=coverage-report/DotCover.html
sonar.cs.opencover.reportsPaths=coverage-report/OpenCover.xml
sonar.cs.vstest.reportsPaths=test-results/**/*.trx

# Frontend specific settings
sonar.javascript.lcov.reportPaths=workflow-platform-frontend/coverage/lcov.info
sonar.typescript.lcov.reportPaths=workflow-platform-frontend/coverage/lcov.info

# Quality gate settings
sonar.qualitygate.wait=true
```

### Add SONAR_TOKEN to repository secrets**

```bash
# In SonarCloud project settings:
# Administration > Security > Generate new token
# Add token to GitHub repository secrets as SONAR_TOKEN
```

### Custom Quality Gates

**Code Coverage Requirements**:

```yaml
# In backend pipeline
- name: Check Code Coverage
  run: |
    COVERAGE=$(grep -o 'line-rate="[^"]*"' coverage.xml | head -1 | grep -o '[0-9.]*')
    COVERAGE_PERCENT=$(echo "$COVERAGE * 100" | bc)
    if (( $(echo "$COVERAGE_PERCENT < 80" | bc -l) )); then
      echo "Code coverage ($COVERAGE_PERCENT%) is below 80% threshold"
      exit 1
    fi
```

**Performance Benchmarks**:

```yaml
# In deployment pipeline
- name: Performance Benchmark
  run: |
    RESPONSE_TIME=$(curl -w '%{time_total}' -s -o /dev/null https://your-app.com/health)
    if (( $(echo "$RESPONSE_TIME > 0.2" | bc -l) )); then
      echo "Response time ($RESPONSE_TIME s) exceeds 200ms threshold"
      exit 1
    fi
```

## Security Configuration

### Container Security Scanning

**Trivy Configuration** (`.trivyignore`):

```yaml
# Ignore specific vulnerabilities (with justification)
CVE-2021-44228  # Log4j - not applicable to .NET applications
CVE-2022-12345  # Fixed in next patch release

# Ignore low severity vulnerabilities in development
--severity HIGH,CRITICAL
```

**Custom Security Policies** (`.github/workflows/security.yml`):

```yaml
name: Security Scan

on:
  pull_request:
  schedule:
    - cron: '0 6 * * *'  # Daily security scan

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Comprehensive Security Scan
        run: |
          # Secret scanning
          truffleHog --regex --entropy=False .
          
          # Dependency vulnerabilities
          npm audit --audit-level moderate
          dotnet list package --vulnerable --include-transitive
          
          # Infrastructure security
          checkov -d . --framework dockerfile,kubernetes
```

### RBAC Configuration

**Kubernetes Service Account** (`k8s/service-account.yaml`):

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: workflow-platform-sa
  namespace: workflow-platform
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: workflow-platform-role
  namespace: workflow-platform
rules:
- apiGroups: [""]
  resources: ["pods", "services", "configmaps", "secrets"]
  verbs: ["get", "list", "create", "update", "patch", "delete"]
- apiGroups: ["apps"]
  resources: ["deployments", "replicasets"]
  verbs: ["get", "list", "create", "update", "patch", "delete"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: workflow-platform-binding
  namespace: workflow-platform
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: workflow-platform-role
subjects:
- kind: ServiceAccount
  name: workflow-platform-sa
  namespace: workflow-platform
```

## Monitoring and Alerting

### Pipeline Monitoring

**GitHub Actions Metrics Collection**:

```yaml
# Add to each workflow
- name: Collect Pipeline Metrics
  if: always()
  run: |
    curl -X POST "${{ secrets.METRICS_ENDPOINT }}" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer ${{ secrets.METRICS_TOKEN }}" \
      -d '{
        "workflow": "${{ github.workflow }}",
        "job": "${{ github.job }}",
        "status": "${{ job.status }}",
        "duration": "${{ steps.timer.outputs.duration }}",
        "commit": "${{ github.sha }}",
        "branch": "${{ github.ref_name }}",
        "actor": "${{ github.actor }}"
      }'
```

### Alerting Configuration

**Slack Notifications**:

```yaml
- name: Notify Slack on Failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: failure
    channel: '#devops-alerts'
    title: 'Pipeline Failure'
    message: |
      Pipeline failed for ${{ github.repository }}
      Branch: ${{ github.ref_name }}
      Commit: ${{ github.sha }}
      Actor: ${{ github.actor }}
    fields: repo,message,commit,author,action,eventName,ref,workflow
  env:
    SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

**Email Notifications**:

```yaml
- name: Send Email on Critical Failure
  if: failure() && github.ref == 'refs/heads/main'
  uses: dawidd6/action-send-mail@v3
  with:
    server_address: smtp.gmail.com
    server_port: 587
    username: ${{ secrets.MAIL_USERNAME }}
    password: ${{ secrets.MAIL_PASSWORD }}
    subject: "CRITICAL: Production Pipeline Failure"
    to: devops-team@company.com
    from: github-actions@company.com
    body: |
      Production pipeline failed for commit ${{ github.sha }}
      
      Repository: ${{ github.repository }}
      Branch: ${{ github.ref_name }}
      Workflow: ${{ github.workflow }}
      
      Please investigate immediately.
```

## Performance Optimization

### Runner Optimization

**Self-Hosted Runners**:

```yaml
# For resource-intensive jobs
jobs:
  build:
    runs-on: self-hosted-large
    # Custom runner with 8 cores, 32GB RAM
```

**Matrix Strategies**:

```yaml
# Parallel testing across multiple configurations
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest]
    dotnet-version: ['8.0.x', '9.0.x']
    test-suite: [unit, integration, e2e]
```

### Caching Strategies

**Advanced Caching**:

```yaml
# Multi-level caching
- name: Cache Dependencies
  uses: actions/cache@v4
  with:
    path: |
      ~/.nuget/packages
      node_modules
      ~/.cargo/registry
    key: ${{ runner.os }}-deps-${{ hashFiles('**/*.csproj', 'package-lock.json', 'Cargo.lock') }}
    restore-keys: |
      ${{ runner.os }}-deps-
      ${{ runner.os }}-
```

## Troubleshooting Configuration

### Debug Mode

Enable debug logging for workflows:

```yaml
# Add to workflow file
env:
  ACTIONS_STEP_DEBUG: true
  ACTIONS_RUNNER_DEBUG: true
```

### Configuration Validation

**Pre-commit Hooks** (`.pre-commit-config.yaml`):

```yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: yaml-lint
        files: \.ya?ml$
      - id: json-lint
        files: \.json$
  - repo: https://github.com/adrienverge/yamllint
    rev: v1.29.0
    hooks:
      - id: yamllint
        args: [-c=.yamllint.yaml]
```

**Workflow Validation Script**:

```bash
#!/bin/bash
# scripts/validate-workflows.sh

echo "Validating GitHub Actions workflows..."

for workflow in .github/workflows/*.yml; do
    echo "Validating $workflow"
    if ! yamllint "$workflow"; then
        echo "❌ YAML syntax error in $workflow"
        exit 1
    fi
done

echo "✅ All workflows are valid"
```

## Best Practices

### Configuration Management

1. **Version Control**: Keep all configuration in version control
2. **Environment Parity**: Maintain consistency between environments
3. **Secret Rotation**: Regularly rotate all secrets and credentials
4. **Documentation**: Document all configuration changes

### Security

1. **Least Privilege**: Grant minimal required permissions
2. **Secret Scanning**: Never commit secrets to repository
3. **Audit Trail**: Log all configuration changes
4. **Regular Reviews**: Review and update configurations monthly

### Performance

1. **Resource Planning**: Right-size runners for workload
2. **Caching Strategy**: Cache at multiple levels for efficiency
3. **Parallel Execution**: Maximize concurrent job execution
4. **Monitoring**: Track and optimize pipeline performance

---

*Next: [Security Guidelines](./security.md)*
