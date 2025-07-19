# Workflow Platform Helm Chart

This Helm chart deploys the Workflow Orchestration Platform, an enterprise-grade workflow automation system built with .NET and React.

## 🔒 Security Notice

**IMPORTANT**: This chart does NOT contain hardcoded secrets or credentials. All sensitive values must be provided externally.

## Quick Start

### Development Environment

```bash
# Install with development values
helm install workflow-platform ./helm/workflow-platform \
  -f helm/workflow-platform/values-dev.yaml \
  --create-namespace \
  --namespace workflow-platform-dev
```

### Production Environment

```bash
# Create secrets first (see secrets.example.yaml)
kubectl apply -f your-production-secrets.yaml

# Install with production values
helm install workflow-platform ./helm/workflow-platform \
  -f helm/workflow-platform/values-prod.yaml \
  --create-namespace \
  --namespace workflow-platform-prod
```

## Prerequisites

- Kubernetes 1.20+
- Helm 3.8+
- PV provisioner support in the underlying infrastructure

## Values Configuration

### Required Values for Production

The following values MUST be set for production deployment:

```yaml
# Database credentials (use existingSecret recommended)
database:
  username: "your-username"
  password: "your-secure-password"
  # OR use existing secret
  existingSecret: "database-secret"

# RabbitMQ credentials  
rabbitmq:
  username: "your-username"
  password: "your-secure-password"
  # OR use existing secret
  existingSecret: "rabbitmq-secret"

# JWT configuration (use existingSecret recommended)
jwt:
  secret: "your-256-bit-secret-key"
  issuer: "your-issuer"
  audience: "your-audience"
  # OR use existing secret
  existingSecret: "jwt-secret"

# PostgreSQL subchart (if enabled)
postgresql:
  auth:
    postgresPassword: "your-postgres-password"
    username: "your-username" 
    password: "your-secure-password"
```

### Recommended Production Configuration

```bash
helm install workflow-platform ./helm/workflow-platform \
  --set global.environment=production \
  --set replicaCount=3 \
  --set api.autoscaling.enabled=true \
  --set database.existingSecret=database-secret \
  --set rabbitmq.existingSecret=rabbitmq-secret \
  --set jwt.existingSecret=jwt-secret \
  --set ingress.enabled=true \
  --set ingress.hosts[0].host=workflow.yourdomain.com \
  --set monitoring.enabled=true \
  --set security.networkPolicy.enabled=true \
  --namespace workflow-platform \
  --create-namespace
```

## Security Configuration

### External Secrets (Recommended)

Use Kubernetes secrets or external secret management:

```yaml
# Use existing secrets
database:
  existingSecret: "database-secret"
  secretKey: "connection-string"

rabbitmq:
  existingSecret: "rabbitmq-secret"
  secretKey: "connection-string"

jwt:
  existingSecret: "jwt-secret"
  secretKeys:
    secret: "jwt-secret-key"
    issuer: "jwt-issuer-key"
    audience: "jwt-audience-key"
```

### Pod Security

The chart enforces security best practices:

- Containers run as non-root user (UID 1000)
- Read-only root filesystem where possible
- Security capabilities dropped
- Resource limits enforced

### Network Security

Enable network policies for production:

```yaml
security:
  networkPolicy:
    enabled: true
```

## Monitoring

Enable monitoring and metrics collection:

```yaml
monitoring:
  enabled: true
  serviceMonitor:
    enabled: true
    interval: 30s
```

## High Availability

For production high availability:

```yaml
replicaCount: 3

api:
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10

# Use external managed services
database:
  external: true
  host: "your-managed-postgres-host"

redis:
  external: true
  host: "your-managed-redis-host"

rabbitmq:
  external: true
  host: "your-managed-rabbitmq-host"

# Disable internal services
postgresql:
  enabled: false

externalServices:
  redis:
    enabled: false
  rabbitmq:
    enabled: false
```

## Upgrading

### From v0.x to v1.0

⚠️ **Breaking Change**: Hardcoded credentials removed

Before upgrading:

1. Create Kubernetes secrets with your credentials
2. Update values to reference the secrets
3. Perform the upgrade

```bash
# Create secrets
kubectl create secret generic database-secret \
  --from-literal=connection-string="Host=...;Password=..." \
  --namespace workflow-platform

# Upgrade
helm upgrade workflow-platform ./helm/workflow-platform \
  --set database.existingSecret=database-secret
```

## Troubleshooting

### Common Issues

**Issue**: Pods failing to start with authentication errors
**Solution**: Verify all required secrets are created and referenced correctly

**Issue**: Database connection failures  
**Solution**: Check database credentials and network connectivity

**Issue**: RabbitMQ connection errors
**Solution**: Verify RabbitMQ credentials and service availability

### Debug Commands

```bash
# Check pod status
kubectl get pods -n workflow-platform

# View pod logs
kubectl logs -f deployment/workflow-platform-api -n workflow-platform

# Check configuration
kubectl describe configmap workflow-platform-config -n workflow-platform

# Test database connectivity
kubectl exec -it deployment/workflow-platform-api -n workflow-platform \
  -- /bin/sh -c "nc -zv postgresql 5432"
```

## Values File Examples

- `values.yaml` - Default values (no secrets)
- `values-dev.yaml` - Development environment example
- `values-prod.yaml.template` - Production template
- `secrets.example.yaml` - Secret creation examples

## Support

For issues and support:

- GitHub Issues: <https://github.com/ribatshepo/WorkFlowOchestrator/issues>
- Documentation: <https://github.com/ribatshepo/WorkFlowOchestrator/tree/main/docs>

## License

This Helm chart is licensed under the Apache License 2.0.
