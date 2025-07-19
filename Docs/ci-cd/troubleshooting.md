# Troubleshooting Guide

## Overview

This guide provides solutions for common issues encountered when working with the CI/CD pipeline for the Workflow Orchestration Platform.

## Pipeline Issues

### Pipeline Not Triggering

#### Symptoms

- Push to repository doesn't trigger workflow
- Pull request creation doesn't start pipeline
- Manual workflow dispatch not available

#### Possible Causes & Solutions

### GitHub Actions Disabled

```bash
# Check: Repository Settings > Actions > General
# Ensure "Allow all actions and reusable workflows" is selected

# Solution: Enable GitHub Actions
# Go to Settings > Actions > General
# Select "Allow all actions and reusable workflows"
```

### Workflow File Syntax Errors

```bash
# Check workflow file syntax
yamllint .github/workflows/main.yml

# Common syntax errors:
# - Incorrect indentation (YAML is indentation-sensitive)
# - Missing required fields (name, on, jobs)
# - Invalid YAML characters

# Solution: Fix syntax errors and recommit
```

### Branch Protection Conflicts**

```bash
# Check branch protection rules
# Repository Settings > Branches > Branch protection rules

# If rules are too restrictive:
# - Temporarily reduce restrictions
# - Ensure service accounts have necessary permissions
# - Check required status checks configuration
```

### Change Detection Not Working

#### Change Detection Symptoms

- Backend/Frontend pipelines not running when expected
- All pipelines running despite limited changes
- Wrong pipeline running for changes

#### Diagnosis & Solutions

**Check Path Filters**:

```yaml
# Verify path filters in .github/workflows/main.yml
- name: Debug Path Filter
  run: |
    echo "Changed files:"
    git diff --name-only HEAD~1 HEAD
    
    echo "Backend filter match:"
    git diff --name-only HEAD~1 HEAD | grep -E '^(src/|tests/|.*\.sln|Directory\.Build\.props)'
    
    echo "Frontend filter match:"
    git diff --name-only HEAD~1 HEAD | grep -E '^workflow-platform-frontend/'
```

**Common Path Filter Issues**:

```yaml
# Issue: Filters too restrictive
filters: |
  backend:
    - 'src/**'  # Might miss root-level changes
  
# Solution: Add more inclusive patterns
filters: |
  backend:
    - 'src/**'
    - 'tests/**'
    - '*.sln'
    - '*.csproj'
    - 'Directory.Build.props'
    - 'global.json'
```

### Build Failures

#### .NET Build Issues

**Dependency Resolution Failures**:

```bash
# Symptoms:
# - "Package 'X' could not be found"
# - "The type or namespace 'X' could not be found"

# Diagnosis:
dotnet restore --verbosity detailed

# Common solutions:
# 1. Clear NuGet cache
dotnet nuget locals all --clear

# 2. Add missing package references
dotnet add package <PackageName>

# 3. Check package source configuration
cat nuget.config
```

**Build Configuration Issues**:

```bash
# Check build configuration
dotnet build --configuration Release --verbosity normal

# Common issues:
# 1. Missing build properties
# 2. Conditional compilation symbols
# 3. Target framework mismatches

# Solution: Review Directory.Build.props and project files
```

#### Node.js Build Issues

**Dependency Installation Failures**:

```bash
# Symptoms:
# - "Cannot resolve dependency 'X'"
# - "ERESOLVE unable to resolve dependency tree"

# Diagnosis:
cd workflow-platform-frontend
npm install --verbose

# Solutions:
# 1. Clear npm cache
npm cache clean --force

# 2. Delete node_modules and package-lock.json
rm -rf node_modules package-lock.json
npm install

# 3. Use legacy peer deps resolver
npm install --legacy-peer-deps
```

**TypeScript Compilation Errors**:

```bash
# Check TypeScript configuration
npx tsc --noEmit --project tsconfig.json

# Common fixes:
# 1. Update type definitions
npm install --save-dev @types/node @types/react

# 2. Fix strict mode issues
# Edit tsconfig.json to adjust strict settings temporarily
```

### Container Build Issues

#### Docker Build Failures

**Context Issues**:

```bash
# Symptoms:
# - "COPY failed: file not found"
# - "No such file or directory"

# Diagnosis: Check Docker context
docker build --no-cache -t test-build .

# Solution: Verify .dockerignore and file paths
cat .dockerignore
ls -la src/WorkflowPlatform.API/
```

**Multi-stage Build Issues**:

```bash
# Common problems:
# 1. Missing files between stages
# 2. Wrong base images
# 3. Permission issues

# Debug multi-stage build:
docker build --target build -t debug-build .
docker run --rm -it debug-build /bin/sh

# Check intermediate stages:
docker images --filter "dangling=true"
```

#### Registry Push Failures

**Authentication Issues**:

```bash
# Symptoms:
# - "authentication required"
# - "unauthorized: access denied"

# Check GitHub token permissions:
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_ACTOR --password-stdin

# Verify repository permissions:
# Settings > Actions > General > Workflow permissions
# Should be set to "Read and write permissions"
```

**Image Size Issues**:

```bash
# Symptoms:
# - "layer does not exist"
# - "insufficient_scope"

# Analyze image size:
docker images | grep $IMAGE_NAME
dive $IMAGE_NAME:$TAG

# Optimize Dockerfile:
# - Use multi-stage builds
# - Minimize layers
# - Clean up in same RUN command
```

## Deployment Issues

### Kubernetes Deployment Failures

#### Authentication Problems

**KUBECONFIG Issues**:

```bash
# Test kubeconfig locally:
echo "$KUBECONFIG_STAGING" | base64 -d > /tmp/kubeconfig
kubectl --kubeconfig=/tmp/kubeconfig get nodes

# Common issues:
# 1. Expired certificates
# 2. Wrong cluster endpoints
# 3. Missing permissions

# Debug in workflow:
- name: Debug Kubernetes Access
  run: |
    kubectl config view --minify
    kubectl auth can-i create deployments --namespace workflow-platform
    kubectl get nodes
```

**RBAC Permission Issues**:

```bash
# Symptoms:
# - "forbidden: User cannot create resource"
# - "error validating data"

# Check service account permissions:
kubectl describe rolebinding workflow-platform-binding -n workflow-platform
kubectl auth can-i '*' '*' --as=system:serviceaccount:workflow-platform:workflow-api-sa

# Create debug pod to test permissions:
kubectl run debug-pod --image=alpine --rm -it -- /bin/sh
```

#### Helm Deployment Issues

**Chart Validation Failures**:

```bash
# Test Helm chart locally:
helm template workflow-platform ./helm/workflow-platform \
  --values ./helm/workflow-platform/values-staging.yaml \
  --debug

# Common issues:
# 1. Missing required values
# 2. Template syntax errors
# 3. Resource conflicts

# Debug Helm deployment:
helm upgrade --install workflow-platform ./helm/workflow-platform \
  --values ./helm/workflow-platform/values-staging.yaml \
  --dry-run --debug
```

**Resource Conflicts**:

```bash
# Symptoms:
# - "resource mapping not found"
# - "the server could not find the requested resource"

# Check resource versions:
kubectl api-versions | grep apps
kubectl explain deployment --api-version=apps/v1

# Verify resource quotas:
kubectl describe resourcequota -n workflow-platform
kubectl top pods -n workflow-platform
```

### Health Check Failures

#### Application Health Issues

**Health Endpoint Not Responding**:

```bash
# Debug health endpoint:
kubectl port-forward deployment/workflow-api 8080:8080 -n workflow-platform
curl -v http://localhost:8080/health

# Check application logs:
kubectl logs deployment/workflow-api -n workflow-platform --tail=100

# Common causes:
# 1. Database connection issues
# 2. Missing configuration
# 3. Startup sequence problems
```

**Database Connection Issues**:

```bash
# Test database connectivity:
kubectl exec -it deployment/workflow-api -n workflow-platform -- /bin/sh
# Inside pod:
nc -zv postgres-service 5432
nslookup postgres-service

# Check database secrets:
kubectl get secret db-secrets -n workflow-platform -o yaml
echo "cG9zdGdyZXM=" | base64 -d  # Decode secret values
```

## Security Issues

### Secret Management Problems

#### Missing Secrets

**GitHub Repository Secrets**:

```bash
# Check if secrets are configured:
# Go to Repository Settings > Secrets and variables > Actions
# Verify all required secrets are present:

# Required secrets checklist:
# - KUBECONFIG_STAGING ✓
# - KUBECONFIG_PRODUCTION ✓  
# - DB_CONNECTION_STRING_STAGING ✓
# - DB_CONNECTION_STRING_PRODUCTION ✓
# - JWT_SECRET_STAGING ✓
# - JWT_SECRET_PRODUCTION ✓
```

**Kubernetes Secret Issues**:

```bash
# Check if secrets exist in cluster:
kubectl get secrets -n workflow-platform
kubectl describe secret app-secrets -n workflow-platform

# Verify secret data:
kubectl get secret app-secrets -n workflow-platform -o jsonpath="{.data}" | base64 -d
```

#### Secret Decoding Issues

**Base64 Encoding Problems**:

```bash
# Test secret encoding/decoding:
echo "test-value" | base64
echo "dGVzdC12YWx1ZQo=" | base64 -d

# Common issues:
# - Newlines in encoded values
# - Wrong encoding format
# - Special characters not escaped

# Solution: Use proper encoding:
cat ~/.kube/config | base64 -w 0  # No line wrapping
```

### Security Scan Failures

#### Container Security Issues

**Trivy Scan Failures**:

```bash
# Run Trivy scan locally:
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image $IMAGE_NAME:$TAG

# Common issues:
# 1. Critical vulnerabilities in base image
# 2. Outdated dependencies
# 3. Configuration issues

# Solutions:
# 1. Update base image
# 2. Update dependencies
# 3. Add vulnerability exceptions (if justified)
```

**CodeQL Analysis Issues**:

```bash
# Debug CodeQL analysis:
# Check workflow logs for specific errors

# Common issues:
# 1. Build failures during analysis
# 2. Language detection problems
# 3. Query timeouts

# Solutions:
# 1. Fix build issues first
# 2. Specify languages explicitly
# 3. Reduce analysis scope
```

## Performance Issues

### Pipeline Performance

#### Slow Build Times

**Diagnosis Steps**:

```yaml
# Add timing to pipeline steps:
- name: Time Build Step
  run: |
    START_TIME=$(date +%s)
    dotnet build --configuration Release
    END_TIME=$(date +%s)
    echo "Build time: $((END_TIME - START_TIME)) seconds"
```

**Common Optimizations**:

**1. Dependency Caching**:

```yaml
# Improve caching strategy:
- name: Cache Dependencies
  uses: actions/cache@v4
  with:
    path: |
      ~/.nuget/packages
      node_modules
    key: ${{ runner.os }}-deps-${{ hashFiles('**/*.csproj', 'package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-deps-
```

**2. Parallel Execution**:

```yaml
# Use build matrix for parallel jobs:
strategy:
  matrix:
    component: [backend, frontend, tests]
    include:
      - component: backend
        path: src/
      - component: frontend  
        path: workflow-platform-frontend/
```

**3. Runner Optimization**:

```yaml
# Use larger runners for intensive tasks:
jobs:
  build:
    runs-on: ubuntu-latest-4-cores  # 4 cores instead of 2
```

#### Resource Limits

**GitHub Actions Limits**:

```bash
# Check usage limits:
# Account Settings > Billing and plans > Usage this month

# Limits to watch:
# - Job execution time: 6 hours max
# - Workflow run time: 35 days max  
# - Concurrent jobs: varies by plan
# - Storage: varies by plan
```

**Resource Optimization**:

```yaml
# Optimize resource usage:
- name: Cleanup Previous Builds
  run: |
    docker system prune -f
    rm -rf /tmp/*
    
- name: Limit Parallel Processes
  run: |
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
    dotnet build -maxcpucount:2
```

## Monitoring and Debugging

### Log Analysis

#### Application Logs

**Structured Log Analysis**:

```bash
# Extract structured logs:
kubectl logs deployment/workflow-api -n workflow-platform | jq .

# Filter by log level:
kubectl logs deployment/workflow-api -n workflow-platform | jq 'select(.level == "Error")'

# Search for specific events:
kubectl logs deployment/workflow-api -n workflow-platform | grep "Authentication failed"
```

#### Pipeline Logs

**GitHub Actions Debug Logs**:

```yaml
# Enable debug logging:
env:
  ACTIONS_STEP_DEBUG: true
  ACTIONS_RUNNER_DEBUG: true

# Access debug logs:
# Go to workflow run > Re-run jobs > Enable debug logging
```

### Monitoring Setup

#### Health Monitoring

**Application Health Dashboard**:

```bash
# Check application metrics:
curl https://your-app.com/metrics

# Prometheus queries for troubleshooting:
# HTTP error rate: rate(http_requests_total{status=~"5.."}[5m])
# Response time: histogram_quantile(0.95, http_request_duration_seconds)
# Memory usage: container_memory_usage_bytes
```

#### Infrastructure Monitoring

**Kubernetes Cluster Health**:

```bash
# Cluster resource usage:
kubectl top nodes
kubectl top pods -n workflow-platform

# Event monitoring:
kubectl get events --sort-by='.lastTimestamp' -n workflow-platform

# Resource quotas:
kubectl describe resourcequota -n workflow-platform
```

## Emergency Procedures

### Pipeline Emergency Stop

#### Stop Running Workflows

**Cancel Active Workflows**:

```bash
# Via GitHub CLI:
gh workflow list
gh run cancel <run-id>

# Via API:
curl -X POST \
  -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID/cancel"
```

#### Emergency Deployment Rollback

**Kubernetes Rollback**:

```bash
# Check rollout history:
kubectl rollout history deployment/workflow-api -n workflow-platform

# Rollback to previous version:
kubectl rollout undo deployment/workflow-api -n workflow-platform

# Rollback to specific revision:
kubectl rollout undo deployment/workflow-api -n workflow-platform --to-revision=2

# Verify rollback:
kubectl rollout status deployment/workflow-api -n workflow-platform
```

**Helm Rollback**:

```bash
# Check release history:
helm history workflow-platform -n workflow-platform

# Rollback to previous release:
helm rollback workflow-platform -n workflow-platform

# Rollback to specific revision:
helm rollback workflow-platform 2 -n workflow-platform
```

### Disaster Recovery

#### Database Recovery

**Backup Restoration**:

```bash
# List available backups:
kubectl get pvc -n workflow-platform
aws s3 ls s3://workflow-platform-backups/

# Restore from backup:
kubectl exec -it postgres-pod -n workflow-platform -- \
  psql -U postgres -d workflow_platform < backup.sql
```

#### Complete Environment Recovery

**Infrastructure Rebuild**:

```bash
# 1. Recreate namespace:
kubectl delete namespace workflow-platform
kubectl create namespace workflow-platform

# 2. Restore secrets:
kubectl apply -f secrets-backup.yaml -n workflow-platform

# 3. Redeploy application:
helm install workflow-platform ./helm/workflow-platform \
  --namespace workflow-platform \
  --values values-production.yaml

# 4. Verify recovery:
kubectl get all -n workflow-platform
curl -f https://your-app.com/health
```

## Getting Help

### Internal Resources

**Team Contacts**:

- **DevOps Team**: `#devops-team` Slack channel
- **Security Team**: `security@company.com`
- **Backend Team**: `#backend-dev` Slack channel
- **Frontend Team**: `#frontend-dev` Slack channel

### External Resources

**Documentation**:

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Helm Documentation](https://helm.sh/docs/)
- [Docker Documentation](https://docs.docker.com/)

**Community Support**:

- [GitHub Actions Community](https://github.com/actions/community)
- [Kubernetes Slack](https://kubernetes.slack.com/)
- [Docker Community Forums](https://forums.docker.com/)

### Creating Support Tickets

**Information to Include**:

```markdown
## Issue Description
Brief description of the problem

## Environment
- Branch: main/develop/feature/xxx
- Workflow: Backend/Frontend/Deploy
- Run ID: 1234567890
- Commit SHA: abc123def456

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Logs and Screenshots
- Relevant log snippets
- Screenshots of error messages
- Configuration files (with secrets removed)

## Troubleshooting Attempted
- What you've already tried
- Results of troubleshooting steps
```

---
