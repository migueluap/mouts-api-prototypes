# Secrets Configuration Guide

This project uses **User Secrets** for local development and **Environment Variables** for production to keep sensitive credentials secure.

## Why Secrets Management?

Credentials should **never** be committed to source control. This project implements secrets management to:

- ✅ Protect database passwords, API keys, and JWT secrets
- ✅ Prevent accidental credential leaks
- ✅ Enable different configurations per environment
- ✅ Follow .NET security best practices

---

## Local Development Setup (User Secrets)

### Step 1: Navigate to the WebApi Project

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi
```

### Step 2: Configure Your Secrets

The project already has a `UserSecretsId` configured. Set your secrets with:

```bash
# Database Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=YOUR_PASSWORD"

# JWT Secret Key (must be at least 32 bytes)
dotnet user-secrets set "Jwt:SecretKey" "YOUR_JWT_SECRET_KEY_HERE"
```

### Step 3: Verify Configuration

```bash
dotnet user-secrets list
```

You should see both secrets listed.

### Where are Secrets Stored?

User Secrets are stored in:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **Linux/macOS**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

These files are **outside your repository** and never committed to git.

---

## Production Deployment (Environment Variables)

For production environments (Docker, Kubernetes, Azure, AWS), use environment variables:

### Docker Compose

```yaml
services:
  api:
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=developer_evaluation;Username=developer;Password=${DB_PASSWORD}
      - Jwt__SecretKey=${JWT_SECRET_KEY}
```

### Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ambev-secrets
type: Opaque
stringData:
  connection-string: "Host=postgres;Port=5432;Database=developer_evaluation;Username=developer;Password=SECURE_PASSWORD"
  jwt-secret: "YOUR_PRODUCTION_JWT_SECRET_KEY"
```

```yaml
# Pod configuration
env:
  - name: ConnectionStrings__DefaultConnection
    valueFrom:
      secretKeyRef:
        name: ambev-secrets
        key: connection-string
  - name: Jwt__SecretKey
    valueFrom:
      secretKeyRef:
        name: ambev-secrets
        key: jwt-secret
```

### Azure App Service

Configure in Azure Portal:
1. Go to Configuration > Application Settings
2. Add:
   - `ConnectionStrings__DefaultConnection`
   - `Jwt__SecretKey`

### AWS ECS/Fargate

Use AWS Secrets Manager or Parameter Store:

```json
{
  "secrets": [
    {
      "name": "ConnectionStrings__DefaultConnection",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:db-connection"
    },
    {
      "name": "Jwt__SecretKey",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:jwt-key"
    }
  ]
}
```

---

## Configuration Hierarchy

.NET configuration sources are loaded in this order (later sources override earlier ones):

1. `appsettings.json` - Base configuration (no secrets)
2. `appsettings.{Environment}.json` - Environment-specific settings
3. **User Secrets** (Development only)
4. **Environment Variables** (Production)
5. Command-line arguments

---

## Required Secrets

### 1. Database Connection String

**Key**: `ConnectionStrings:DefaultConnection`

**Format**:
```
Host=<hostname>;Port=<port>;Database=<database_name>;Username=<username>;Password=<password>
```

**Example**:
```
Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=SecureP@ssw0rd
```

### 2. JWT Secret Key

**Key**: `Jwt:SecretKey`

**Requirements**:
- Minimum 32 bytes (256 bits)
- Use cryptographically secure random string
- Different keys for each environment

**Generate Secure Key** (PowerShell):
```powershell
$bytes = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

**Generate Secure Key** (Bash):
```bash
openssl rand -base64 32
```

---

## Files in This Repository

- ✅ `appsettings.json` - Base configuration with **empty** secrets (safe to commit)
- ✅ `appsettings.Template.json` - Template showing required structure (safe to commit)
- ❌ `secrets.json` - Never commit (ignored by .gitignore)
- ❌ `appsettings.*.local.json` - Never commit (ignored by .gitignore)

---

## Troubleshooting

### Application fails to start with configuration error

**Problem**: Missing required secrets

**Solution**: Ensure you've configured User Secrets:
```bash
cd src/Ambev.DeveloperEvaluation.WebApi
dotnet user-secrets list
```

If empty, follow the setup steps above.

### Database connection fails

**Problem**: Incorrect connection string

**Solution**: Verify your connection string includes the correct password:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=YOUR_CORRECT_PASSWORD"
```

### JWT token generation fails

**Problem**: JWT SecretKey is too short or missing

**Solution**: Set a key with at least 32 bytes:
```bash
dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -base64 32)"
```

---

## Security Best Practices

1. **Never commit secrets** to version control
2. **Use different secrets** for each environment (dev, staging, prod)
3. **Rotate secrets regularly** (especially after team member changes)
4. **Restrict access** to production secrets
5. **Use managed secret stores** in production (Azure Key Vault, AWS Secrets Manager, etc.)
6. **Enable audit logging** for secret access in production

---

## Additional Resources

- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
