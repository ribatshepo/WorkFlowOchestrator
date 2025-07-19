# 🔒 Security Configuration Guide

## 🚨 Important Security Notice

### NO SENSITIVE INFORMATION SHOULD BE HARDCODED IN SOURCE CODE

All sensitive configuration values have been removed from source control and must be configured through secure methods:

---

## 🛡️ Development Environment Setup

### 1. Configure User Secrets (Recommended for Development)

```bash
# Initialize User Secrets (if not already done)
cd src/WorkflowPlatform.API
dotnet user-secrets init

# Database Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=WorkflowPlatformDev;Username=your_dev_user;Password=your_dev_password;Port=5432"

# JWT Configuration
dotnet user-secrets set "Jwt:Secret" "your-development-jwt-secret-key-must-be-at-least-256-bits-long"
dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform-Dev"
dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform-Dev-Users"
```

### 2. Verify User Secrets Configuration

```bash
# List all configured secrets
dotnet user-secrets list

# Should show:
# ConnectionStrings:DefaultConnection = Host=localhost;Database=...
# Jwt:Secret = your-development-jwt-secret...
# Jwt:Issuer = WorkflowPlatform-Dev
# Jwt:Audience = WorkflowPlatform-Dev-Users
```

---

## 🏭 Production Environment Setup

### 1. Environment Variables (Docker/Container Deployment)

```bash
# Database Configuration
export ConnectionStrings__DefaultConnection="Host=your-prod-db-server;Database=WorkflowPlatform;Username=your-prod-user;Password=your-secure-prod-password;Port=5432;SSL Mode=Require"

# JWT Configuration
export Jwt__Secret="your-super-secure-production-jwt-secret-key-minimum-256-bits"
export Jwt__Issuer="WorkflowPlatform-Production"
export Jwt__Audience="WorkflowPlatform-Production-Users"
```

### 2. Azure App Service Configuration

```bash
# Add these as Application Settings in Azure Portal
ConnectionStrings__DefaultConnection = "your-azure-sql-connection-string"
Jwt__Secret = "your-production-jwt-secret"
Jwt__Issuer = "WorkflowPlatform-Production"
Jwt__Audience = "WorkflowPlatform-Production-Users"
```

### 3. AWS / Other Cloud Providers

Use the cloud provider's secrets management service:

- **AWS**: AWS Secrets Manager or Parameter Store
- **Azure**: Azure Key Vault
- **Google Cloud**: Secret Manager

---

## 🔐 Security Requirements

### JWT Secret Requirements

- **Minimum Length**: 256 bits (32 characters)
- **Complexity**: Use cryptographically secure random generation
- **Rotation**: Change regularly in production
- **Storage**: Never store in source code, logs, or unsecured locations

### Database Password Requirements

- **Strong Password**: Minimum 12 characters with mixed case, numbers, symbols
- **Dedicated User**: Don't use admin/root accounts
- **Limited Permissions**: Grant only required database permissions
- **Network Security**: Use SSL/TLS for database connections

---

## 🚀 Startup Validation

The application will **fail to start** if required secrets are not configured:

```bash
# Missing JWT Secret
InvalidOperationException: JWT Secret is not configured. Please set it in User Secrets or Environment Variables.

# Missing JWT Issuer
InvalidOperationException: JWT Issuer is not configured.

# Missing JWT Audience  
InvalidOperationException: JWT Audience is not configured.
```

---

## 🛠️ Development Team Setup

### First-Time Setup Checklist

- [ ] Clone repository
- [ ] Install .NET 8 SDK
- [ ] Install PostgreSQL locally
- [ ] **Copy `.env.template` to `.env` and set secure passwords**
- [ ] Configure User Secrets (see commands above)
- [ ] Run `dotnet build` to verify configuration
- [ ] Run `dotnet run` to start application
- [ ] Access `/swagger` to verify API is running

### ⚠️ **CRITICAL**: Environment File Setup

**BEFORE running docker-compose or setup scripts:**

1. **Copy the template file:**

   ```bash
   cp .env.template .env
   ```

2. **Edit `.env` with secure values:**

   ```bash
   # Set strong passwords (minimum 12 characters)
   POSTGRES_PASSWORD=your_very_secure_postgres_password_here
   REDIS_PASSWORD=your_very_secure_redis_password_here
   RABBITMQ_PASSWORD=your_very_secure_rabbitmq_password_here
   GRAFANA_PASSWORD=your_very_secure_grafana_password_here
   JWT_SECRET=your_256_bit_jwt_secret_key_change_this_immediately
   ```

3. **NEVER commit `.env` files to source control**

### Team Secrets Sharing (Secure Methods Only)

**✅ Acceptable Methods:**

- Password managers (1Password, Bitwarden, etc.)
- Secure team chat (encrypted channels)
- In-person or secure video call

**❌ Never Use:**

- Email
- Slack/Teams unencrypted messages
- Source control commits
- Documentation files in repository

---

## � CI/CD Pipeline Security

### GitHub Repository Secrets

**Required for production deployment:**

```bash
# Production Database
DATABASE_CONNECTION_STRING="Host=prod-db;Database=WorkflowPlatform;Username=prod_user;Password=SECURE_PASSWORD;Port=5432;SSL Mode=Require"

# Production Redis  
REDIS_CONNECTION_STRING="prod-redis:6379,password=SECURE_REDIS_PASSWORD"

# Production RabbitMQ
RABBITMQ_CONNECTION_STRING="amqp://prod_user:SECURE_RABBITMQ_PASSWORD@prod-rabbitmq:5672/"

# JWT Configuration
JWT_SECRET="PRODUCTION_JWT_SECRET_MINIMUM_256_BITS"
JWT_ISSUER="WorkflowPlatform-Production"
JWT_AUDIENCE="WorkflowPlatform-Production-Users"

# Kubernetes Access
KUBECONFIG="base64_encoded_kubeconfig_file"

# Code Quality
SONAR_TOKEN="your_sonarcloud_token"
```

### CI/CD Security Best Practices

- **✅ DO**: Use GitHub repository secrets for all sensitive data
- **✅ DO**: Use different secrets for staging and production environments  
- **✅ DO**: Rotate secrets regularly
- **✅ DO**: Use environment-specific namespaces in Kubernetes
- **✅ DO**: Enable branch protection rules on main/develop branches

- **❌ NEVER**: Hardcode secrets in workflow files
- **❌ NEVER**: Log secrets in CI/CD output
- **❌ NEVER**: Use development secrets in production
- **❌ NEVER**: Commit `.env` files to source control

### Security Validation in CI/CD

The pipeline includes:

- **CodeQL Security Scanning** - Automated vulnerability detection
- **Trivy Container Scanning** - Docker image security analysis  
- **SonarCloud Integration** - Code quality and security gates
- **SBOM Generation** - Software Bill of Materials
- **Container Image Signing** - Cosign-based verification

---

## �🔍 Security Validation

### Pre-Commit Checks

Before committing code, ensure:

- [ ] No hardcoded passwords or secrets
- [ ] No connection strings with real credentials
- [ ] Configuration files use placeholder values
- [ ] User Secrets are configured locally

### Code Review Security Checklist

- [ ] No sensitive information in code changes
- [ ] Configuration follows secure patterns
- [ ] Secrets are externalized properly
- [ ] Environment-specific configuration is correct

---

## 📚 Additional Security Resources

### JWT Best Practices

- Use strong, unique secrets per environment
- Implement token refresh mechanisms
- Set appropriate expiration times
- Monitor for token abuse

### Database Security

- Use connection pooling
- Implement query parameterization (EF Core handles this)
- Regular security updates
- Database audit logging

### Configuration Security

- Separate secrets per environment
- Use managed identity where available
- Implement secret rotation
- Monitor configuration access

---

**Remember: Security is everyone's responsibility!** 🔐
