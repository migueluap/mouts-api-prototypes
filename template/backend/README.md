# Ambev Developer Evaluation - Sales API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-133%20Passing-success)](./tests)

Enterprise-grade Sales Management API built with Clean Architecture, DDD, and CQRS patterns.

## 📝 Description

RESTful API for managing sales operations with advanced features including:
- Complete CRUD operations for Sales and SaleItems
- Automatic discount calculation based on quantity rules
- Optimistic concurrency control for safe concurrent operations
- Domain events for auditing and integration
- Advanced filtering with wildcards, ranges, and pagination

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Start PostgreSQL + API
docker compose up -d

# Check logs
docker logs ambev_developer_evaluation_webapi -f

# API available at: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

### Option 2: Local Development

```bash
# 1. Start PostgreSQL (or use Docker)
docker compose up -d ambev.developerevaluation.database

# 2. Configure User Secrets (see Configuration section)

# 3. Run migrations
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi

# 4. Run the API
cd src/Ambev.DeveloperEvaluation.WebApi
dotnet run --urls "http://localhost:5000"

# API available at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (recommended) or PostgreSQL 16+
- [Git](https://git-scm.com/)

## ⚙️ Configuration

### User Secrets Setup

For local development without Docker:

```bash
# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"

# Set JWT secret key
dotnet user-secrets set "Jwt:SecretKey" "YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong"

# Verify secrets
dotnet user-secrets list
```

**Important**: Never commit secrets to Git. See [SECRETS.md](./SECRETS.md) for detailed security guidelines.

## 🏗️ Architecture

Built with **Clean Architecture** and **Domain-Driven Design (DDD)**:

```
┌─────────────────────────────────────────┐
│  WebApi (Presentation Layer)           │  ← Controllers, DTOs, Middleware
├─────────────────────────────────────────┤
│  Application (Use Cases)                │  ← CQRS Handlers, Validators
├─────────────────────────────────────────┤
│  Domain (Business Logic)                │  ← Entities, Events, Rules
├─────────────────────────────────────────┤
│  Infrastructure (Data Access)           │  ← EF Core, Repositories
└─────────────────────────────────────────┘
```

**Key Patterns**:
- **CQRS**: Separate Commands and Queries with MediatR
- **Domain Events**: Event-driven architecture for auditing and integration
- **Repository Pattern**: Abstract data access
- **External Identity Pattern**: Denormalization for microservices autonomy
- **Optimistic Concurrency**: RowVersion-based conflict detection

## ✨ Features

### Business Rules
- **4-tier discount system**:
  - Items 4-9: 10% discount
  - Items 10-20: 20% discount
  - Maximum 20 items per sale
  - Automatic total calculation with discounts

### API Capabilities
- **CRUD Operations**: Create, Read, Update, Cancel, Delete sales
- **Advanced Filtering**: Wildcards (`SALE*`, `*001`), date ranges, amount ranges
- **Pagination & Ordering**: Flexible result sets with custom sorting
- **Concurrency Control**: Prevent lost updates with RowVersion
- **Domain Events**: Track all sale lifecycle changes (Created, Modified, Cancelled)

## 🧪 Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Build and test
dotnet build && dotnet test --no-build
```

**Test Coverage**:
- ✅ 133 unit tests passing (100%)
- Domain entities and business rules
- CQRS handlers (Commands + Queries)
- Validators (FluentValidation)
- Repository operations

## 📡 API Endpoints

### Sales Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/Sales` | Create a new sale |
| `GET` | `/api/Sales/{id}` | Get sale by ID |
| `GET` | `/api/Sales` | Get all sales (with filters) |
| `PUT` | `/api/Sales/{id}` | Update sale |
| `POST` | `/api/Sales/{id}/cancel` | Cancel sale |
| `DELETE` | `/api/Sales/{id}` | Delete sale |

### Example: Create Sale

```bash
curl -X POST http://localhost:5000/api/Sales \
  -H "Content-Type: application/json" \
  -d '{
    "saleNumber": "SALE-2026-001",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "branchId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "branchName": "Main Branch",
    "date": "2026-09-23T00:00:00Z",
    "items": [{
      "productId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
      "productName": "Product A",
      "quantity": 10,
      "unitPrice": 100
    }]
  }'
```

**Response**: Sale with automatic 20% discount applied (10 items)

### Query Filters

```bash
# Wildcard search
GET /api/Sales?saleNumber=SALE*

# Date range
GET /api/Sales?_minDate=2026-01-01&_maxDate=2026-12-31

# Amount range
GET /api/Sales?_minTotalAmount=100&_maxTotalAmount=500

# Pagination
GET /api/Sales?_page=1&_size=10

# Ordering
GET /api/Sales?_order=date desc,saleNumber asc

# Combined filters
GET /api/Sales?customerName=John*&_minTotalAmount=200&isCancelled=false
```

See [API_TESTS.md](./API_TESTS.md) for complete examples and test results.

## 🐳 Docker

### Docker Compose Services

```yaml
services:
  database:    # PostgreSQL 16
  webapi:      # Sales API (.NET 8)
```

### Useful Commands

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f webapi

# Stop services
docker compose down

# Rebuild images
docker compose build --no-cache

# Database shell
docker exec -it ambev_developer_evaluation_database psql -U developer -d developer_evaluation
```

## 🛠️ Technologies

- **.NET 8**: Latest LTS framework
- **ASP.NET Core**: Web API with minimal APIs style
- **Entity Framework Core 9**: ORM with Code-First migrations
- **PostgreSQL 16**: Relational database
- **MediatR**: CQRS implementation
- **FluentValidation**: Business rules validation
- **AutoMapper**: Object-to-object mapping
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization
- **xUnit**: Unit testing framework

## 📂 Project Structure

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/       # Entities, Events, Validation
│   ├── Ambev.DeveloperEvaluation.Application/  # CQRS Handlers, DTOs
│   ├── Ambev.DeveloperEvaluation.ORM/          # EF Core, Repositories
│   ├── Ambev.DeveloperEvaluation.IoC/          # Dependency Injection
│   ├── Ambev.DeveloperEvaluation.WebApi/       # Controllers, Middleware
│   └── Ambev.DeveloperEvaluation.Common/       # Shared utilities
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit/         # Unit tests (133 tests)
│   ├── Ambev.DeveloperEvaluation.Integration/  # Integration tests
│   └── Ambev.DeveloperEvaluation.Functional/   # Functional tests
├── Ambev.DeveloperEvaluation.sln
├── docker-compose.yml
├── Directory.Packages.props                     # Central Package Management
└── README.md
```

## 🔒 Design Patterns & Practices

### Domain-Driven Design (DDD)
- **Entities**: Sale, SaleItem with business rules
- **Value Objects**: Strongly-typed identifiers
- **Domain Events**: SaleCreated, SaleModified, SaleCancelled, ItemCancelled
- **Aggregates**: Sale as aggregate root containing SaleItems
- **Repositories**: Abstraction over data access

### CQRS (Command Query Responsibility Segregation)
- **Commands**: CreateSale, UpdateSale, CancelSale, DeleteSale
- **Queries**: GetSale, GetAllSales
- **Handlers**: One handler per command/query

### Other Patterns
- **External Identity Pattern**: Reference external entities without foreign keys
- **Repository Pattern**: Abstract data persistence
- **Mediator Pattern**: Decouple request/response with MediatR
- **Unit of Work**: Transaction management via EF Core DbContext

## 🐛 Troubleshooting

### Database connection issues

```bash
# Check PostgreSQL container is running
docker ps

# Check connection
docker exec ambev_developer_evaluation_database pg_isready -U developer

# View database logs
docker logs ambev_developer_evaluation_database
```

### API not starting

```bash
# Check port availability
netstat -ano | findstr :5000

# Verify appsettings.json or User Secrets are configured
dotnet user-secrets list

# Check build errors
dotnet build --verbosity detailed
```

### Tests failing

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
dotnet test
```

### Port conflicts

Edit `docker-compose.override.yml` or `launchSettings.json` to change ports.

---

**Project**: Ambev Developer Evaluation
**Version**: 1.0.0
**Date**: 2026-09-23
