# API Reference

## Overview

The CI/CD pipeline provides several APIs and integration points for monitoring, control, and automation. This document covers all available interfaces including webhook endpoints, monitoring APIs, and programmatic access methods.

## Pipeline Control APIs

### GitHub Actions API Integration

#### Workflow Triggers

**Manual Workflow Dispatch**:

```bash
curl -X POST \
  -H "Authorization: token $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/workflows/main.yml/dispatches" \
  -d '{
    "ref": "main",
    "inputs": {
      "force_backend": "true",
      "force_frontend": "false"
    }
  }'
```

**Query Workflow Runs**:

```bash
# Get workflow runs
curl -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/runs?status=completed&per_page=10"

# Get specific run details
curl -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID"

# Get run artifacts
curl -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID/artifacts"
```

**Cancel Running Workflows**:

```bash
curl -X POST \
  -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID/cancel"
```

### Workflow Status Webhooks

#### Repository Webhooks

**Configuration**:

```json
{
  "name": "web",
  "active": true,
  "events": [
    "workflow_run",
    "deployment_status",
    "check_run"
  ],
  "config": {
    "url": "https://your-webhook-endpoint.com/github-webhook",
    "content_type": "json",
    "secret": "your-webhook-secret"
  }
}
```

**Webhook Payload Examples**:

**Workflow Run Event**:

```json
{
  "action": "completed",
  "workflow_run": {
    "id": 123456789,
    "name": "Main CI/CD Pipeline",
    "status": "completed",
    "conclusion": "success",
    "workflow_id": 987654321,
    "check_suite_id": 456789123,
    "created_at": "2025-07-19T10:00:00Z",
    "updated_at": "2025-07-19T10:15:00Z",
    "run_started_at": "2025-07-19T10:00:30Z"
  },
  "repository": {
    "id": 123456789,
    "name": "WorkFlowOchestrator",
    "full_name": "ribatshepo/WorkFlowOchestrator"
  }
}
```

**Deployment Status Event**:

```json
{
  "action": "created",
  "deployment": {
    "id": 123456,
    "sha": "abc123def456",
    "ref": "main",
    "environment": "production",
    "created_at": "2025-07-19T10:15:00Z"
  },
  "deployment_status": {
    "id": 789012,
    "state": "success",
    "target_url": "https://your-app.com",
    "environment": "production"
  }
}
```

### Custom Pipeline APIs

#### Pipeline Status API

**Endpoint**: `GET /api/v1/pipeline/status`

**Response**:

```json
{
  "pipeline_id": "main-ci-cd-pipeline",
  "status": "running",
  "current_stage": "deploy-production",
  "started_at": "2025-07-19T10:00:00Z",
  "estimated_completion": "2025-07-19T10:20:00Z",
  "stages": [
    {
      "name": "backend-pipeline",
      "status": "completed",
      "duration": 180,
      "artifacts": ["api-image:sha-abc123"]
    },
    {
      "name": "frontend-pipeline", 
      "status": "completed",
      "duration": 120,
      "artifacts": ["frontend-image:sha-def456"]
    },
    {
      "name": "deploy-production",
      "status": "running",
      "started_at": "2025-07-19T10:15:00Z"
    }
  ]
}
```

#### Pipeline Metrics API

**Endpoint**: `GET /api/v1/pipeline/metrics`

**Query Parameters**:

- `timeframe`: `1h`, `24h`, `7d`, `30d`
- `workflow`: `backend`, `frontend`, `deploy`
- `branch`: `main`, `develop`, `feature/*`

**Response**:

```json
{
  "timeframe": "24h",
  "metrics": {
    "total_runs": 45,
    "success_rate": 0.95,
    "average_duration": 420,
    "failure_count": 2,
    "deployment_frequency": 8,
    "lead_time_minutes": 15,
    "recovery_time_minutes": 3
  },
  "breakdown_by_workflow": {
    "backend-pipeline": {
      "runs": 20,
      "success_rate": 0.95,
      "average_duration": 240
    },
    "frontend-pipeline": {
      "runs": 20, 
      "success_rate": 1.0,
      "average_duration": 180
    },
    "deploy": {
      "runs": 5,
      "success_rate": 0.8,
      "average_duration": 300
    }
  }
}
```

## Monitoring Integration APIs

### Prometheus Metrics

#### Pipeline Metrics Exposition

**Endpoint**: `GET /metrics` (Prometheus format)

**Available Metrics**:

```prometheus
# Pipeline execution metrics
github_actions_workflow_runs_total{workflow="main",status="success"} 45
github_actions_workflow_runs_total{workflow="main",status="failure"} 2
github_actions_workflow_duration_seconds{workflow="backend",quantile="0.5"} 180
github_actions_workflow_duration_seconds{workflow="backend",quantile="0.95"} 240

# Deployment metrics
deployment_frequency_per_day{environment="production"} 2.5
deployment_success_rate{environment="staging"} 0.98
deployment_lead_time_minutes{environment="production"} 15

# Quality metrics
code_coverage_percentage{project="backend"} 85.5
security_vulnerabilities_total{severity="high"} 0
technical_debt_minutes{component="frontend"} 45

# Infrastructure metrics
container_registry_images_total{repository="workflow-platform"} 156
kubernetes_deployments_total{namespace="workflow-platform"} 3
helm_releases_total{namespace="workflow-platform"} 1
```

#### Custom Metrics Collection

**PromQL Queries for Dashboards**:

```promql
# Pipeline success rate over time
rate(github_actions_workflow_runs_total{status="success"}[5m]) / 
rate(github_actions_workflow_runs_total[5m])

# Average build time trend
rate(github_actions_workflow_duration_seconds_sum[5m]) / 
rate(github_actions_workflow_duration_seconds_count[5m])

# Deployment frequency
increase(deployment_frequency_per_day[1d])

# Security vulnerability trend
deriv(security_vulnerabilities_total[1h])
```

### Grafana Dashboard API

#### Dashboard Configuration

**Dashboard Import JSON**:

```json
{
  "dashboard": {
    "id": null,
    "title": "CI/CD Pipeline Metrics",
    "tags": ["cicd", "workflow-platform"],
    "timezone": "UTC",
    "panels": [
      {
        "id": 1,
        "title": "Pipeline Success Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(github_actions_workflow_runs_total{status=\"success\"}[5m]) / rate(github_actions_workflow_runs_total[5m])",
            "legendFormat": "Success Rate"
          }
        ]
      },
      {
        "id": 2,
        "title": "Build Duration Trend",
        "type": "graph",
        "targets": [
          {
            "expr": "github_actions_workflow_duration_seconds{quantile=\"0.95\"}",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ],
    "time": {
      "from": "now-24h",
      "to": "now"
    }
  }
}
```

### Alerting API Integration

#### Slack Integration

**Webhook Payload Format**:

```json
{
  "channel": "#devops-alerts",
  "username": "GitHub Actions",
  "icon_emoji": ":github:",
  "text": "Pipeline Alert",
  "attachments": [
    {
      "color": "danger",
      "title": "Pipeline Failure: Backend Build",
      "title_link": "https://github.com/ribatshepo/WorkFlowOchestrator/actions/runs/123456789",
      "fields": [
        {
          "title": "Repository",
          "value": "ribatshepo/WorkFlowOchestrator",
          "short": true
        },
        {
          "title": "Branch",
          "value": "main",
          "short": true
        },
        {
          "title": "Commit",
          "value": "abc123d",
          "short": true
        },
        {
          "title": "Actor",
          "value": "developer@example.com",
          "short": true
        }
      ],
      "footer": "GitHub Actions",
      "ts": 1642617600
    }
  ]
}
```

#### Microsoft Teams Integration

**Webhook Payload Format**:

```json
{
  "@type": "MessageCard",
  "@context": "https://schema.org/extensions",
  "summary": "Pipeline Failure Alert",
  "themeColor": "FF0000",
  "title": "CI/CD Pipeline Failed",
  "sections": [
    {
      "activityTitle": "Main CI/CD Pipeline",
      "activitySubtitle": "ribatshepo/WorkFlowOchestrator",
      "facts": [
        {
          "name": "Status",
          "value": "Failed"
        },
        {
          "name": "Branch", 
          "value": "main"
        },
        {
          "name": "Commit",
          "value": "abc123def456"
        },
        {
          "name": "Duration",
          "value": "5m 32s"
        }
      ]
    }
  ],
  "potentialAction": [
    {
      "@type": "OpenUri",
      "name": "View Pipeline",
      "targets": [
        {
          "os": "default",
          "uri": "https://github.com/ribatshepo/WorkFlowOchestrator/actions/runs/123456789"
        }
      ]
    }
  ]
}
```

## Container Registry APIs

### GitHub Container Registry

#### Authentication

**Token-based Authentication**:

```bash
# Login to registry
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_ACTOR --password-stdin

# API authentication
curl -H "Authorization: Bearer $GITHUB_TOKEN" \
  "https://ghcr.io/v2/"
```

#### Image Management

**List Repository Images**:

```bash
curl -H "Authorization: Bearer $GITHUB_TOKEN" \
  "https://api.github.com/user/packages/container/workfloworchestrator/versions"
```

**Get Image Metadata**:

```bash
curl -H "Authorization: Bearer $GITHUB_TOKEN" \
  "https://ghcr.io/v2/ribatshepo/workfloworchestrator/manifests/latest"
```

**Delete Image Version**:

```bash
curl -X DELETE \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  "https://api.github.com/user/packages/container/workfloworchestrator/versions/$VERSION_ID"
```

### Image Scanning API

#### Trivy Scan Results

**API Endpoint**: `GET /api/v1/security/scan-results/{image_id}`

**Response Format**:

```json
{
  "image_id": "ghcr.io/ribatshepo/workfloworchestrator:abc123",
  "scan_date": "2025-07-19T10:00:00Z",
  "scanner": "trivy",
  "scanner_version": "0.45.0",
  "summary": {
    "total_vulnerabilities": 3,
    "critical": 0,
    "high": 1,
    "medium": 2,
    "low": 0
  },
  "vulnerabilities": [
    {
      "id": "CVE-2023-12345",
      "severity": "HIGH",
      "package": "openssl",
      "installed_version": "1.1.1k",
      "fixed_version": "1.1.1l",
      "description": "Buffer overflow vulnerability in OpenSSL",
      "references": [
        "https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2023-12345"
      ]
    }
  ],
  "compliance": {
    "passed": true,
    "policies": ["security-baseline", "company-standards"],
    "exceptions": []
  }
}
```

## Deployment APIs

### Kubernetes API Integration

#### Deployment Status

**Check Deployment Status**:

```bash
curl -H "Authorization: Bearer $KUBE_TOKEN" \
  "https://kubernetes.example.com/api/v1/namespaces/workflow-platform/deployments/workflow-api" \
  | jq '.status'
```

**Scale Deployment**:

```bash
curl -X PATCH \
  -H "Authorization: Bearer $KUBE_TOKEN" \
  -H "Content-Type: application/merge-patch+json" \
  "https://kubernetes.example.com/api/v1/namespaces/workflow-platform/deployments/workflow-api" \
  -d '{"spec":{"replicas":5}}'
```

#### Pod Management

**Get Pod Logs**:

```bash
curl -H "Authorization: Bearer $KUBE_TOKEN" \
  "https://kubernetes.example.com/api/v1/namespaces/workflow-platform/pods/$POD_NAME/log?tailLines=100"
```

**Execute Commands in Pod**:

```bash
# WebSocket connection required for exec
curl -H "Authorization: Bearer $KUBE_TOKEN" \
  -H "Connection: Upgrade" \
  -H "Upgrade: websocket" \
  "https://kubernetes.example.com/api/v1/namespaces/workflow-platform/pods/$POD_NAME/exec?command=sh&stdin=true&stdout=true"
```

### Helm API Integration

#### Release Management

**List Releases**:

```bash
helm list -n workflow-platform -o json
```

**Release History**:

```bash
helm history workflow-platform -n workflow-platform -o json
```

**Rollback Release**:

```bash
curl -X POST \
  -H "Authorization: Bearer $HELM_TOKEN" \
  "https://tiller.example.com/api/v1/releases/workflow-platform/rollback" \
  -d '{"revision": 2}'
```

## Health Check APIs

### Application Health Endpoints

#### Primary Health Check

**Endpoint**: `GET /health`

**Response**:

```json
{
  "status": "Healthy",
  "timestamp": "2025-07-19T10:00:00Z",
  "version": "1.0.0",
  "environment": "production",
  "uptime": "5d 3h 42m",
  "checks": {
    "database": {
      "status": "Healthy",
      "response_time": "15ms",
      "connection_pool": {
        "active": 5,
        "idle": 10,
        "max": 50
      }
    },
    "redis": {
      "status": "Healthy", 
      "response_time": "2ms",
      "memory_usage": "45%"
    },
    "message_queue": {
      "status": "Healthy",
      "queue_depth": 12,
      "consumer_count": 3
    }
  }
}
```

#### Detailed Health Check

**Endpoint**: `GET /health/detailed`

**Response**:

```json
{
  "status": "Healthy",
  "components": {
    "workflow_engine": {
      "status": "Healthy",
      "active_executions": 25,
      "queued_executions": 8,
      "completed_today": 342
    },
    "node_strategies": {
      "status": "Healthy",
      "registered_strategies": 15,
      "active_strategies": 12,
      "failed_strategies": 0
    },
    "external_integrations": {
      "sendgrid": {
        "status": "Healthy",
        "last_check": "2025-07-19T09:55:00Z",
        "api_quota_remaining": 85
      },
      "azure_services": {
        "status": "Degraded",
        "last_check": "2025-07-19T09:45:00Z",
        "error": "High latency detected"
      }
    }
  }
}
```

### Infrastructure Health

#### Kubernetes Cluster Health

**Endpoint**: `GET /api/v1/infrastructure/health`

**Response**:

```json
{
  "cluster_status": "Healthy",
  "nodes": {
    "total": 5,
    "ready": 5,
    "not_ready": 0
  },
  "workloads": {
    "deployments": {
      "total": 3,
      "available": 3,
      "unavailable": 0
    },
    "pods": {
      "total": 15,
      "running": 15,
      "pending": 0,
      "failed": 0
    }
  },
  "resources": {
    "cpu_usage": "45%",
    "memory_usage": "60%",
    "storage_usage": "30%"
  }
}
```

## Authentication & Authorization

### API Authentication

#### GitHub Token Authentication

```bash
# Personal Access Token
curl -H "Authorization: token ghp_xxxxxxxxxxxxxxxxxxxx" \
  "https://api.github.com/repos/ribatshepo/WorkFlowOchestrator"

# GitHub App Authentication
curl -H "Authorization: Bearer $JWT_TOKEN" \
  "https://api.github.com/app/installations"
```

#### Service Account Authentication

**JWT Token Generation**:

```bash
# Generate JWT for service account
JWT_HEADER='{"typ":"JWT","alg":"HS256"}'
JWT_PAYLOAD='{"sub":"pipeline-service","iat":'$(date +%s)',"exp":'$(($(date +%s) + 3600))'}'

JWT_HEADER_B64=$(echo -n "$JWT_HEADER" | base64 -w 0)
JWT_PAYLOAD_B64=$(echo -n "$JWT_PAYLOAD" | base64 -w 0)

JWT_SIGNATURE=$(echo -n "$JWT_HEADER_B64.$JWT_PAYLOAD_B64" | \
  openssl dgst -sha256 -hmac "$JWT_SECRET" -binary | base64 -w 0)

JWT_TOKEN="$JWT_HEADER_B64.$JWT_PAYLOAD_B64.$JWT_SIGNATURE"
```

### API Rate Limits

#### GitHub API Limits

```json
{
  "resources": {
    "core": {
      "limit": 5000,
      "remaining": 4999,
      "reset": 1642617600,
      "used": 1
    },
    "search": {
      "limit": 30,
      "remaining": 30,
      "reset": 1642617600,
      "used": 0
    }
  }
}
```

#### Custom API Rate Limits

```json
{
  "rate_limit": {
    "requests_per_minute": 100,
    "requests_remaining": 95,
    "reset_time": "2025-07-19T10:01:00Z",
    "burst_limit": 200
  }
}
```

## SDK and Client Libraries

### Pipeline SDK

#### Node.js Client

```javascript
const { PipelineClient } = require('@workflow-platform/pipeline-sdk');

const client = new PipelineClient({
  baseUrl: 'https://api.workflowplatform.com',
  apiKey: process.env.PIPELINE_API_KEY
});

// Trigger pipeline
await client.triggerPipeline({
  workflow: 'main',
  branch: 'feature/new-feature',
  parameters: {
    force_backend: true,
    force_frontend: false
  }
});

// Monitor pipeline status
const status = await client.getPipelineStatus('run-123456');
console.log(`Pipeline status: ${status.current_stage}`);
```

#### Python Client

```python
from workflow_platform import PipelineClient

client = PipelineClient(
    base_url='https://api.workflowplatform.com',
    api_key=os.environ['PIPELINE_API_KEY']
)

# Trigger pipeline
response = client.trigger_pipeline(
    workflow='main',
    branch='feature/new-feature',
    parameters={
        'force_backend': True,
        'force_frontend': False
    }
)

# Monitor pipeline
status = client.get_pipeline_status('run-123456')
print(f"Pipeline status: {status.current_stage}")
```

### Webhook Handlers

#### Express.js Webhook Handler

```javascript
const express = require('express');
const crypto = require('crypto');

const app = express();
app.use(express.json());

app.post('/webhook/github', (req, res) => {
  const signature = req.headers['x-hub-signature-256'];
  const payload = JSON.stringify(req.body);
  
  // Verify webhook signature
  const expectedSignature = crypto
    .createHmac('sha256', process.env.WEBHOOK_SECRET)
    .update(payload)
    .digest('hex');
    
  if (!crypto.timingSafeEqual(
    Buffer.from(`sha256=${expectedSignature}`),
    Buffer.from(signature)
  )) {
    return res.status(401).send('Unauthorized');
  }
  
  // Process webhook
  const { action, workflow_run } = req.body;
  
  if (action === 'completed') {
    console.log(`Workflow ${workflow_run.name} completed with status: ${workflow_run.conclusion}`);
    
    // Custom logic here
    await notifyTeam(workflow_run);
  }
  
  res.status(200).send('OK');
});
```
