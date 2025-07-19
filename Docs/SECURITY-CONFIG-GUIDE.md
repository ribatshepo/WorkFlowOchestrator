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
- [ ] Configure User Secrets (see commands above)
- [ ] Run `dotnet build` to verify configuration
- [ ] Run `dotnet run` to start application
- [ ] Access `/swagger` to verify API is running

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

## 🔍 Security Validation

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
