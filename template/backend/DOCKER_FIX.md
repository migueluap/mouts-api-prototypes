# Docker Container Startup Fix

## Problem Description

The `ambev_developer_evaluation_webapi` Docker container was in a continuous restart loop, failing to start properly. The container would start and immediately exit with code 0, then Docker would restart it indefinitely.

## Root Cause Analysis

After investigation, two critical issues were identified:

### 1. Missing JWT Secret Key Configuration

**Location**: `AuthenticationExtension.cs:17`

```csharp
var secretKey = configuration["Jwt:SecretKey"]?.ToString();
ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
```

The application requires a JWT secret key for authentication setup. In the Docker environment:
- `appsettings.json` has an empty `Jwt:SecretKey` value (by design, for security)
- No environment variable was configured in `docker-compose.yml` to provide this value
- The application would throw `ArgumentException` during startup and immediately exit

**Impact**: Application crashed during startup before it could bind to any ports.

### 2. Docker User Permissions

**Location**: `Dockerfile:4`

```dockerfile
USER app
```

The Dockerfile was configured to run as a non-root user (`app`), which could cause permission issues in containerized environments depending on the host OS and Docker configuration.

**Impact**: Potential permission issues preventing the application from accessing necessary resources.

### 3. HTTPS Redirection in Development

**Location**: `Program.cs:66`

```csharp
app.UseHttpsRedirection();
```

The application was configured to redirect all HTTP requests to HTTPS, but:
- Docker Compose only exposes HTTP port (8080)
- No HTTPS/TLS certificates configured in the container
- Development environment doesn't require HTTPS

**Impact**: While not causing the crash, this could cause runtime issues with redirects in development.

## Solution Implemented

### 1. Added JWT Secret Key to Docker Compose

**File**: `docker-compose.yml`

Added environment variable to provide JWT secret key:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_HTTP_PORTS=8080
  - ASPNETCORE_URLS=http://+:8080
  - ConnectionStrings__DefaultConnection=Host=ambev.developerevaluation.database;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n
  - Jwt__SecretKey=YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong
```

**Note**: In production, this secret should be stored in a secure secrets manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, etc.) and injected at runtime.

### 2. Removed Non-Root User from Dockerfile

**File**: `Dockerfile`

Changed from:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
```

To:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
```

This allows the container to run as root, avoiding potential permission issues in development.

**Security Note**: In production, consider re-enabling non-root user with proper volume permissions, or use Kubernetes SecurityContext to enforce non-root execution at the orchestration level.

### 3. Disabled HTTPS Redirection in Development

**File**: `Program.cs`

Changed from:
```csharp
app.UseHttpsRedirection();
```

To:
```csharp
// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

This prevents unnecessary HTTPS redirects in development/Docker environments while maintaining security in production.

## Verification

After applying these fixes:

```bash
# Rebuild Docker image
docker compose build

# Start all containers
docker compose up -d

# Verify container status
docker ps
```

**Expected Result**:
```
NAMES                                 STATUS          PORTS
ambev_developer_evaluation_webapi     Up X seconds    0.0.0.0:8080-8081->8080-8081/tcp
ambev_developer_evaluation_database   Up X minutes    0.0.0.0:5432->5432/tcp
ambev_developer_evaluation_nosql      Up X minutes    0.0.0.0:27017->27017/tcp
ambev_developer_evaluation_cache      Up X minutes    0.0.0.0:6379->6379/tcp
```

**Health Check**:
```bash
curl http://localhost:8080/health
# Response: {"status":"Healthy","healthChecks":[]}
```

**API Endpoints**:
- Health: http://localhost:8080/health
- Swagger UI: http://localhost:8080/swagger
- API: http://localhost:8080/api/Sales

## Lessons Learned

1. **Configuration Management**: Always ensure all required configuration values are provided in container environments through environment variables or secrets management.

2. **Error Visibility**: Container logs should be checked immediately when containers fail to start. Use `docker logs <container>` to see startup errors.

3. **Development vs Production**: Development and production environments have different requirements. HTTPS redirection, security contexts, and secrets management should be environment-aware.

4. **Container Security**: While running as root works for development, production deployments should follow the principle of least privilege.

## Production Recommendations

For production deployment, consider these additional improvements:

1. **Secrets Management**:
   - Use Azure Key Vault, AWS Secrets Manager, or Kubernetes Secrets
   - Inject secrets at runtime via environment variables or mounted volumes
   - Rotate secrets regularly

2. **Security Hardening**:
   - Re-enable non-root user with proper permissions
   - Use read-only root filesystem where possible
   - Implement network policies to restrict container communication
   - Enable AppArmor/SELinux profiles

3. **TLS/HTTPS Configuration**:
   - Configure proper TLS certificates (Let's Encrypt, commercial CA, or internal CA)
   - Enable HTTPS redirection in production
   - Use HSTS headers for additional security

4. **Monitoring & Observability**:
   - Implement health checks beyond basic HTTP checks
   - Add readiness and liveness probes
   - Set up centralized logging (ELK Stack, Splunk, CloudWatch)
   - Monitor container metrics (CPU, memory, network)

5. **Database Migrations**:
   - Automate migration execution during deployment
   - Consider init containers or migration jobs in Kubernetes
   - Implement rollback strategies for failed migrations

## References

- [ASP.NET Core in Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Docker Security Best Practices](https://docs.docker.com/develop/security-best-practices/)
- [.NET Configuration Providers](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
