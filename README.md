# Multi-Tenant ASP.NET Core Application

A robust multi-tenant ASP.NET Core application using database-per-tenant architecture with enhanced performance, security, and monitoring features.

## 🏗️ Architecture Overview

This application implements a **Database-per-Tenant** multi-tenancy pattern where:
- Each tenant has their own dedicated database
- Tenant resolution is based on HTTP request host/domain
- Centralized tenant metadata management
- Enhanced caching and performance optimizations

## 🚀 Features

### Core Multi-Tenancy
- ✅ Database-per-tenant isolation
- ✅ Domain-based tenant resolution
- ✅ Dynamic database context creation
- ✅ Tenant metadata management

### Performance & Caching
- ✅ In-memory caching for tenant resolution
- ✅ Database connection pooling
- ✅ Async/await throughout
- ✅ Connection health testing

### Monitoring & Health Checks
- ✅ Application health checks
- ✅ Per-tenant database health monitoring
- ✅ Structured logging with Serilog
- ✅ Performance metrics

### Security
- ✅ Connection string masking in logs
- ✅ Input validation
- ✅ Secure error handling
- ✅ CORS configuration

### Developer Experience
- ✅ Swagger/OpenAPI documentation
- ✅ Docker support
- ✅ Development environment setup
- ✅ Comprehensive error messages

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 7.0
- **Database**: MySQL 8.0+ / 9.0+
- **ORM**: Entity Framework Core
- **Logging**: Serilog
- **Caching**: MemoryCache
- **Containerization**: Docker
- **API Documentation**: Swagger/OpenAPI

## 🏃‍♂️ Quick Start

### Prerequisites
- .NET 7.0 SDK
- MySQL Server
- Docker (optional)

### 1. Clone the repository
```bash
git clone <repository-url>
cd MuiltiTenant
```

### 2. Update connection strings
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;Port=3306;user=root;password=yourpassword;database=MultiTenantDb;"
  }
}
```

### 3. Install dependencies
```bash
dotnet restore
```

### 4. Run database migrations
```bash
dotnet ef database update --context ApplicationDbContext
```

### 5. Run the application
```bash
dotnet run
```

### 6. Test the endpoints
- API Documentation: `https://localhost:7067`
- Health Check: `https://localhost:7067/health`
- Tenant Info: `https://localhost:7067/api/MuiltiTenant/tenant-info`

## 🐳 Docker Setup

### Using Docker Compose (Recommended)
```bash
# Start all services (MySQL + API)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Manual Docker Build
```bash
# Build image
docker build -t multitenant-api .

# Run container
docker run -d -p 5075:80 -p 7067:443 multitenant-api
```

## 🗄️ Database Configuration

### Main Database (ApplicationDbContext)
- **Purpose**: Store tenant metadata
- **Tables**: `Tenants`, `Roles`
- **Connection**: Configured in `appsettings.json`

### Tenant Databases (TenantDbContext)
- **Purpose**: Store tenant-specific data
- **Tables**: `Tokens` (example)
- **Connection**: Dynamic, based on tenant configuration

### Default Tenants
The system seeds two default tenants:
1. **ssprintl** - Domain: `localhost`
2. **infisquare** - Domain: `localhost2`

## 📡 API Endpoints

### Tenant Management
```http
GET /api/MuiltiTenant/tenant-info          # Get current tenant info
GET /api/MuiltiTenant/health                # Tenant health check
```

### Token Management
```http
GET /api/MuiltiTenant/tokens                # Get all tokens
GET /api/MuiltiTenant/tokens/{id}           # Get specific token
POST /api/MuiltiTenant/tokens               # Create new token
```

### System Health
```http
GET /health                                 # Overall system health
```

## 🔧 Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;Port=3306;..."
  },
  "TenantSettings": {
    "CacheExpirationMinutes": 30,
    "ConnectionTimeoutSeconds": 120,
    "DefaultMySqlVersion": "9.0.1"
  },
  "AllowedOrigins": ["http://localhost:3000"],
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="server=localhost;..."
```

## 🔍 Monitoring & Logging

### Health Checks
- Application health: `/health`
- Database connectivity for all tenants
- Tenant resolution functionality

### Logging
- **Console**: Development environment
- **File**: `logs/multitenant-YYYY-MM-DD.log`
- **Structured**: JSON format with correlation IDs

### Metrics
- Tenant resolution performance
- Database connection health
- Request/response timing

## 🧪 Testing

### Testing Different Tenants
Use different hosts in your requests:

```bash
# Test tenant 1 (ssprintl)
curl -H "Host: localhost" https://localhost:7067/api/MuiltiTenant/tokens

# Test tenant 2 (infisquare)  
curl -H "Host: localhost2" https://localhost:7067/api/MuiltiTenant/tokens
```

### Health Check Testing
```bash
# Overall health
curl https://localhost:7067/health

# Tenant-specific health
curl -H "Host: localhost" https://localhost:7067/api/MuiltiTenant/health
```

## 🚀 Deployment

### Production Checklist
- [ ] Update connection strings
- [ ] Configure logging levels
- [ ] Set up SSL certificates
- [ ] Configure CORS origins
- [ ] Set up monitoring
- [ ] Database backups
- [ ] Security scanning

### Environment Variables for Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
ConnectionStrings__DefaultConnection="Production connection string"
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

## 📝 Migration from Previous Version

### Breaking Changes
None - all improvements are backward compatible.

### Recommended Updates
1. Update `appsettings.json` with new configuration sections
2. Install new NuGet packages
3. Update any custom tenant resolution logic if applicable

## 🔒 Security Considerations

- Connection strings are masked in logs
- Input validation on all endpoints
- Secure error handling (no internal details exposed)
- CORS properly configured
- Database connections properly disposed

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙋‍♂️ Support

For support and questions:
- Create an issue in the repository
- Check existing documentation
- Review logs for troubleshooting

---

**Made with ❤️ using ASP.NET Core**
