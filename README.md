# ConfigMgr Web Service - Modern REST API

Modern .NET 8 REST API for Microsoft ConfigMgr (SCCM), Intune, and Entra ID operations.

## 🏗️ Architecture

**Clean Architecture** with separation of concerns:

```
src/
├── ConfigMgrWebService.Api/          # Web API Layer (Controllers, Middleware)
├── ConfigMgrWebService.Core/         # Business Logic Layer (Services)
├── ConfigMgrWebService.Infrastructure/ # External Dependencies (ConfigMgr, Graph API)
└── ConfigMgrWebService.Shared/       # Shared Models (DTOs, Responses)
```

## ✨ Features

- ✅ **Modern .NET 8** Web API
- ✅ **REST** instead of SOAP
- ✅ **Windows Authentication** (via IIS)
- ✅ **API Key Authentication** (for system-to-system calls)
- ✅ **Structured Logging** with Serilog
- ✅ **Correlation IDs** for request tracking
- ✅ **Global Exception Handling**
- ✅ **Swagger/OpenAPI** documentation
- ✅ **Health Checks**
- ✅ **Proper HTTP Status Codes**
- ✅ **HTTPS/TLS** transport security

## 🔐 Authentication

### Windows Authentication
Integrated Windows Authentication for user access:
```http
GET /api/v1/computer/MYPC-001/exists
Authorization: Negotiate <token>
```

### API Key Authentication
For system-to-system calls:
```http
GET /api/v1/computer/MYPC-001/exists
X-API-Key: your-secret-api-key-here
```

## 📋 Prerequisites

- **.NET 8 SDK** (or later)
- **IIS 10+** with ASP.NET Core Hosting Bundle
- **Windows Server 2016+** (for IIS deployment)
- **ConfigMgr Site Server** access
- **Azure AD App Registration** (for Graph API)

## ⚙️ Configuration

### appsettings.json

Configure in `src/ConfigMgrWebService.Api/appsettings.json`:

```json
{
  "AppSettings": {
    "ConfigMgr": {
      "SiteServer": "your-sccm-server.domain.com",
      "DomainShortName": "DOMAIN"
    },
    "GraphApi": {
      "AppId": "your-app-id-guid",
      "TenantId": "your-tenant-id-guid",
      "SecretString": "your-client-secret"
    },
    "Authentication": {
      "ApiKeys": {
        "secret-key-1": "External System 1"
      }
    }
  }
}
```

## 🚀 Quick Start

### Development
```bash
cd src/ConfigMgrWebService.Api
dotnet run
```

Access Swagger: `https://localhost:5001/swagger`

### IIS Deployment
See detailed deployment instructions below.

## 📚 API Endpoints

All endpoints return standardized responses:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully",
  "errors": [],
  "correlationId": "abc123",
  "timestamp": "2025-11-17T10:30:00Z"
}
```

### Computer Operations
- `POST /api/v1/computer/add-by-bios-guid` - Add computer
- `DELETE /api/v1/computer/{name}` - Delete computer
- `GET /api/v1/computer/{name}/exists` - Check existence
- `POST /api/v1/computer/{name}/clear-pxe-flag` - Clear PXE flag

See `/swagger` for complete API documentation.

## 📝 Status

**✅ Completed:**
- Project structure & architecture
- DTOs & Response models
- Authentication (Windows + API Key)
- Logging infrastructure (Serilog)
- Exception handling middleware
- Request/Response logging
- Swagger documentation
- IIS deployment configuration

**🚧 TODO:**
- Port GraphUtil to Infrastructure layer
- Port ConfigMgrUtility to Infrastructure layer
- Implement Service layer (business logic)
- Implement remaining controllers
- Add unit tests
- Add integration tests

## 📄 License

Copyright (c) 2025 Swisscom AG