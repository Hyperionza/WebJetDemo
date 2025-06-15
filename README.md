# Movie Price Comparison Application

A full-stack web application that compares movie prices from multiple providers (Cinemaworld and Filmworld) and displays the cheapest option to users. Built with a resilient architecture that remains functional even when external APIs are unavailable.

## 🎯 Features

- **Price Comparison**: Compare movie prices across Cinemaworld and Filmworld
- **Best Price Display**: Automatically highlights the cheapest option
- **Resilient Design**: Continues working even if one or both providers are down
- **Real-time Data**: Fresh price data with caching for performance
- **Responsive UI**: Works seamlessly on desktop and mobile devices
- **Health Monitoring**: API health indicators show provider status

## 🏗️ Architecture

### Backend (.NET 9)
- **ASP.NET Core Web API** with Entity Framework Core
- **Code-First Database** approach with SQL Server
- **Memory Caching** to reduce external API calls
- **Circuit Breaker Pattern** for resilient external API calls
- **Health Checks** for monitoring system status
- **Comprehensive Logging** with Serilog

### Frontend (React 18)
- **TypeScript** for type safety
- **Modern React** with hooks and functional components
- **Responsive Design** with CSS Grid and Flexbox
- **Error Boundaries** for graceful error handling
- **Loading States** and user feedback

### Infrastructure
- **Azure App Service** for backend hosting
- **Azure Static Web Apps** for frontend hosting
- **Azure SQL Database** for data persistence
- **GitHub Actions** for CI/CD pipeline

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- SQL Server (LocalDB for development)
- Git

### Backend Setup
```bash
cd movie-price-api
dotnet restore
dotnet ef database update
dotnet run
```

The API will be available at `https://localhost:7001`

### Frontend Setup
```bash
cd movie-price-frontend
npm install
npm start
```

The frontend will be available at `http://localhost:3000`

### Environment Variables
Create `appsettings.Development.json` in the backend:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MoviePriceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
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
cd movie-price-api
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
- `GET /health` - Health check endpoint
- `GET /api/health` - API health status

### Caching Strategy
- **Movie List**: Cached for 5 minutes
- **Movie Details**: Cached for 10 minutes
- **Provider Health**: Cached for 1 minute

### Error Handling
- **Circuit Breaker**: Prevents cascading failures
- **Graceful Degradation**: Shows cached data when APIs are down
- **User-Friendly Messages**: Clear error states in the UI

## 🏛️ Project Structure

```
├── movie-price-api/              # .NET Backend
│   ├── Controllers/              # API Controllers
│   ├── Models/                   # Data Models
│   ├── Services/                 # Business Logic
│   ├── Data/                     # Entity Framework Context
│   └── Tests/                    # Unit Tests
├── movie-price-frontend/         # React Frontend
│   ├── src/
│   │   ├── components/           # React Components
│   │   ├── services/             # API Services
│   │   ├── types/                # TypeScript Types
│   │   └── __tests__/            # Component Tests
└── .github/workflows/            # CI/CD Pipelines
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

### Movie Response Format
```json
{
  "id": 1,
  "title": "The Matrix",
  "year": "1999",
  "genre": "Action, Sci-Fi",
  "director": "Wachowski Sisters",
  "poster": "https://example.com/poster.jpg",
  "rating": "8.7",
  "bestPrice": {
    "provider": "Filmworld",
    "price": 14.99,
    "freshness": "Fresh",
    "freshnessIndicator": "🟢"
  },
  "prices": [
    {
      "provider": "Cinemaworld",
      "price": 15.99,
      "freshness": "Fresh",
      "freshnessIndicator": "🟢"
    },
    {
      "provider": "Filmworld", 
      "price": 14.99,
      "freshness": "Fresh",
      "freshnessIndicator": "🟢"
    }
  ]
}
```

## 🐛 Known Issues

- Frontend test: "displays placeholder when poster is not available" has assertion mismatch
- Occasional timeout on external API calls during high load

## 📄 License

This project is proprietary software. All rights reserved. See the [LICENSE](LICENSE) file for details.

**NOTICE**: This repository is made publicly available for portfolio demonstration and educational purposes only. Unauthorized copying, distribution, or commercial use is strictly prohibited.

## 🙏 Acknowledgments

- WebJet for providing the external API endpoints
- The open-source community for the excellent tools and libraries used
