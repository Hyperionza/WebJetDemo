# Movie Price Comparison Application

A full-stack web application that compares movie prices from multiple providers (Cinemaworld and Filmworld) and displays the cheapest option to users. Built with a resilient architecture that remains functional even when external APIs are unavailable.

## 🎯 Features

- **Price Comparison**: Compare movie prices across Cinemaworld and Filmworld
- **Best Price Display**: Automatically highlights the cheapest option
- **Dynamic API Providers**: Configurable external API providers with runtime updates
- **Resilient Design**: Continues working even if one or both providers are down
- **Real-time Data**: Fresh price data with caching for performance
- **Responsive UI**: Works seamlessly on desktop and mobile devices
- **Health Monitoring**: API health indicators show provider status

## 🏗️ Architecture

### Backend (.NET 9)
- **ASP.NET Core Web API** with Clean Architecture principles
- **Dynamic API Provider System** with configurable external providers
- **Redis Distributed Cache** for performance optimization
- **In-memory data storage** with repository pattern
- **Health Checks** for monitoring system status
- **Comprehensive Logging** with structured logging

### Frontend (React 18)
- **TypeScript** for type safety
- **Modern React** with hooks and functional components
- **Simple responsive design** with CSS
- **Basic error handling** and loading states
- **Direct API integration** with fetch

### Infrastructure
- **Docker containerization** for both frontend and backend
- **Development container** support with SQL Server and Redis
- **GitHub Actions** for CI/CD pipeline

## 🚀 Quick Start

### Prerequisites
- Docker Desktop (recommended for dev container)
- .NET 9 SDK (if not using dev container)
- Node.js 22+
- Git

### Development Setup

#### Option 1: Dev Container (Recommended)
1. **Open in VS Code**: Open the project folder in VS Code
2. **Reopen in Container**: When prompted, click "Reopen in Container" or use Command Palette > "Dev Containers: Reopen in Container"
3. **Wait for Setup**: The container will automatically set up SQL Server 2022 and Redis 7
4. **Start Development**: All services will be available automatically

#### Option 2: Manual Setup
If you encounter network issues with the dev container or prefer manual setup:

**Backend Setup:**
```bash
cd MoviePriceComparison
dotnet restore
dotnet run
```

**Frontend Setup:**
```bash
cd movie-price-frontend
npm install
npm start
```

The API will be available at `https://localhost:5091` and frontend at `http://localhost:3000`

#### Troubleshooting Dev Container Issues

**Network Issues:**
If you encounter Docker network timeouts when pulling images:
1. **Check Docker Hub connectivity**: Ensure you can access registry-1.docker.io
2. **Retry setup**: Sometimes network issues are temporary - try reopening the container
3. **Use manual setup**: Fall back to Option 2 above
4. **Check corporate firewall**: Some networks block Docker Hub access

**File Permission Issues:**
If you encounter build errors related to file permissions:
1. **Run fix-permissions**: Use the `fix-permissions` alias in the terminal
2. **Clean build artifacts**: The post-create script automatically cleans bin/obj folders
3. **Rebuild container**: If issues persist, rebuild the dev container

**Package/Build Issues:**
If you encounter missing package references or build errors:
1. **Clean and restore**: Run `dotnet clean && dotnet restore --force` in the MoviePriceComparison directory
2. **Rebuild**: Run `dotnet build` to ensure all packages are properly restored
3. **Check dependencies**: Ensure all NuGet packages are properly installed

**Convenient Aliases:**
The dev container provides these helpful aliases:
- `test-api` - Run the .NET API from anywhere
- `test-frontend` - Run the React frontend from anywhere  
- `fix-permissions` - Fix workspace file permissions

### Environment Configuration

The application supports multiple environments with consistent infrastructure:

#### Local Development (Dev Container - Recommended)
- **Database**: SQL Server 2022 (via dev container)
- **Caching**: Redis 7 (via dev container)
- **API Configuration**: Mock configuration service endpoint

#### Production
- **Database**: Azure SQL Database
- **Caching**: Azure Redis Cache
- **API Configuration**: External configuration service

### Environment Variables

#### Development (with Dev Container - Recommended)
The dev container automatically configures SQL Server 2022 and Redis. Connection strings are set via docker-compose:
```json
{
  "Environment": "LOCALDEV",
  "ConnectionStrings": {
    "DefaultConnection": "Server=db,1433;Database=MoviePriceDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;",
    "Redis": "redis:6379"
  },
  "ApiProviderService": {
    "ConfigurationServiceUrl": "http://localhost:5091/api/MockConfiguration/api-providers"
  },
  "ExternalApis": {
    "ApiToken": "your-api-token-here",
    "CinemaworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
    "FilmworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/filmworld"
  }
}
```

#### Development (without Dev Container)
For local development without the dev container, use SQL Server LocalDB and in-memory cache:
```json
{
  "Environment": "Development",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MoviePriceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "ApiProviderService": {
    "ConfigurationServiceUrl": "https://your-config-service.com/api/providers"
  },
  "ExternalApis": {
    "ApiToken": "your-api-token-here",
    "CinemaworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
    "FilmworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/filmworld"
  }
}
```

## 🧪 Testing

### Backend Tests
```bash
cd MoviePriceComparison.Tests
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests
```bash
cd movie-price-frontend
npm test -- --coverage --watchAll=false
```

**Current Test Coverage:**
- Backend: 85%+ code coverage
- Frontend: 71 passed, 1 failed (98.6% pass rate)

## 📦 Deployment

### CI/CD Pipeline
The application uses GitHub Actions for automated deployment:

- **CI Pipeline**: Runs on every push to `main` and all pull requests
- **Staging Deployment**: Triggered by pushes to `staging` branch
- **Production Deployment**: Triggered by pushes to `production` branch

### Manual Deployment
Developers can trigger deployments manually from any branch using GitHub Actions workflow dispatch.

### Branch Strategy
```
main → staging → production
```

## 🔧 Configuration

### API Endpoints
- `GET /api/movies` - Get all movies with price comparison
- `GET /api/movies/{id}` - Get specific movie details
- `POST /api/refresh` - Refresh movie data from external APIs
- `GET /health` - Health check endpoint with environment info
- `GET /api/providers` - Get current API provider configurations
- `POST /api/providers/refresh` - Refresh API provider cache

### Caching Strategy
- **API Provider Configuration**: Cached for 15 minutes
- **Movie Data**: Redis distributed cache with configurable expiration
- **Development**: Redis 7 via dev container
- **Production**: Azure Redis Cache for scalability
- **Provider Health**: Cached for 1 minute

### Error Handling
- **Circuit Breaker**: Prevents cascading failures
- **Graceful Degradation**: Shows cached data when APIs are down
- **User-Friendly Messages**: Clear error states in the UI

## 🏛️ Project Structure

```
├── MoviePriceComparison/         # .NET Backend (Clean Architecture)
│   ├── Controllers/              # API Controllers
│   ├── Application/              # Use Cases & Application Logic
│   │   ├── UseCases/            # Clean Architecture Use Cases
│   │   └── ViewModels/          # DTOs and Response Models
│   ├── Domain/                   # Domain Models & Interfaces
│   │   ├── Entities/            # Domain Entities (MovieSummary, MovieProviderDetail)
│   │   ├── Repositories/        # Repository Interfaces
│   │   └── Services/            # Domain Service Interfaces
│   ├── Infrastructure/           # External Concerns
│   │   ├── Repositories/        # Repository Implementations
│   │   └── Services/            # External Service Implementations
│   ├── Program.cs               # Application entry point & DI configuration
│   ├── appsettings.json         # Configuration
│   └── Dockerfile               # Container configuration
├── MoviePriceComparison.Tests/   # Backend Unit Tests
├── movie-price-frontend/         # React Frontend
│   ├── src/
│   │   ├── components/           # React Components (MovieCard)
│   │   ├── services/             # API Services (movieApi)
│   │   ├── types/                # TypeScript Types (Movie interfaces)
│   │   └── __tests__/            # Component Tests
│   ├── public/                   # Static assets
│   ├── package.json              # Dependencies and scripts
│   └── Dockerfile                # Container configuration
├── .github/workflows/            # CI/CD Pipelines
├── .devcontainer/                # Development Container Config
└── README files for each component
```

## 🔒 Security

- **API Token Management**: Secure handling of external API tokens
- **CORS Configuration**: Properly configured for production
- **Input Validation**: Comprehensive validation on all endpoints
- **Security Headers**: Implemented security best practices
- **Vulnerability Scanning**: Automated security scans in CI/CD

## 📊 Monitoring

### Health Checks
- Database connectivity
- External API availability
- Memory usage
- Response times

### Logging
- Structured logging with Serilog
- Request/response logging
- Error tracking
- Performance metrics

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Write tests for new features
- Update documentation as needed
- Ensure all CI checks pass

## 📝 API Documentation

## 📄 License

This project is proprietary software. All rights reserved. See the [LICENSE](LICENSE) file for details.

**NOTICE**: This repository is made publicly available for portfolio demonstration and educational purposes only. Unauthorized copying, distribution, or commercial use is strictly prohibited.

## 🙏 Acknowledgments

- WebJet for providing the external API endpoints
- The open-source community for the excellent tools and libraries used
