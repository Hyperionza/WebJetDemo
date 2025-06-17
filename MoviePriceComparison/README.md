# Movie Price Comparison API - Clean Architecture

A RESTful API built with **Clean Architecture** principles that compares movie prices from multiple providers (Cinemaworld and Filmworld). The API is designed to be resilient, testable, and maintainable.

## 🏗️ Clean Architecture Implementation

### **Architecture Overview**

This API follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │   Controllers   │  │   API Models    │                  │
│  └─────────────────┘  └─────────────────┘                  │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Use Cases     │  │      DTOs       │  │  Interfaces  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │    Entities     │  │  Value Objects  │  │  Interfaces  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  Repositories   │  │  External APIs  │  │   Database   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### **Project Structure**

```
MoviePriceComparison/
├── Domain/                        # 🎯 Core Business Logic (No Dependencies)
│   ├── Entities/
│   │   ├── Movie.cs              # Movie aggregate root with business logic
│   │   └── MoviePrice.cs         # Value object with price logic
│   ├── Repositories/
│   │   └── IMovieRepository.cs   # Repository abstraction
│   └── Services/
│       └── IExternalMovieApiService.cs # External service abstraction
├── Application/                   # 📋 Use Cases & Application Logic
│   ├── DTOs/
│   │   └── MovieComparisonDto.cs # Data transfer objects
│   └── UseCases/
│       ├── GetMoviesWithPricesUseCase.cs
│       └── GetMovieDetailUseCase.cs
├── Infrastructure/               # 🔧 External Concerns
│   ├── Data/
│   │   └── MovieDbContext.cs    # EF Core implementation
│   ├── Repositories/
│   │   └── MovieRepository.cs   # Repository implementation
│   └── Services/
│       └── ExternalMovieApiService.cs # External API implementation
├── Controllers/                  # 🌐 Presentation Layer
│   └── MoviesController.cs      # Clean controllers using use cases
├── Program.cs                   # Application entry point & DI configuration
├── appsettings.json            # Configuration
└── Dockerfile                  # Container configuration
```

## 🎯 Domain Layer

### **Domain Entities**

#### **MovieSummary Entity**
```csharp
public class MovieSummary
{
    public string Title { get; set; } = null!;
    public string? Year { get; set; }
    public string? Type { get; set; }
    public string? Rated { get; set; }
    public string? Released { get; set; }
    public string? Runtime { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Writer { get; set; }
    public string? Actors { get; set; }
    public string? Plot { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string? Awards { get; set; }
    public string? Metascore { get; set; }
    public string? Rating { get; set; }
    public string? Votes { get; set; }
    public List<MovieProviderDetail> ProviderSpecificDetails { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public MovieProviderDetail? GetCheapestPrice()
    {
        return ProviderSpecificDetails
            .Where(p => p.Price.HasValue && p.Price > 0)
            .OrderBy(p => p.Price!.Value)
            .FirstOrDefault();
    }
    
    public void AddDetail(MovieProviderDetail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail));

        // Remove previous if exists
        ProviderSpecificDetails.RemoveAll(x => x.ProviderId == detail.ProviderId && x.MovieId == detail.MovieId);
        ProviderSpecificDetails.Add(detail);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

#### **MovieProviderDetail Entity**
```csharp
public class MovieProviderDetail
{
    public required string ProviderId { get; set; }
    public required string MovieId { get; set; }
    public required string Provider { get; set; }
    public string? PosterUrl { get; set; }
    public decimal? Price { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsStale(TimeSpan maxAge)
    {
        return DateTime.UtcNow - UpdatedAt > maxAge;
    }
}
```

### **Repository Abstractions**
```csharp
public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllWithPricesAsync();
    Task<Movie?> GetByIdWithPricesAsync(int id);
    Task<IEnumerable<Movie>> SearchAsync(string query);
    Task<Movie> AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
}
```

## 📋 Application Layer

### **Use Cases (CQRS Pattern)**

#### **Query Use Cases**
```csharp
public interface IGetMoviesWithPricesUseCase
{
    Task<IEnumerable<MovieComparisonDto>> ExecuteAsync();
}

public class GetMoviesWithPricesUseCase : IGetMoviesWithPricesUseCase
{
    private readonly IMovieRepository _movieRepository;
    
    public async Task<IEnumerable<MovieComparisonDto>> ExecuteAsync()
    {
        var movies = await _movieRepository.GetAllWithPricesAsync();
        
        return movies.Select(movie => new MovieComparisonDto
        {
            Id = movie.Id,
            Title = movie.Title,
            CheapestPrice = MapToDto(movie.GetCheapestPrice()),
        });
    }
}
```

### **Data Transfer Objects**
```csharp
public class MovieComparisonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<MoviePriceDto> Prices { get; set; } = new();
    public MoviePriceDto? CheapestPrice { get; set; }
}
```

## 🔧 Infrastructure Layer

### **Repository Implementation**
```csharp
public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _context;
    
    public async Task<IEnumerable<Movie>> GetAllWithPricesAsync()
    {
        return await _context.Movies
            .Include(m => m.MoviePrices)
            .OrderBy(m => m.Title)
            .ToListAsync();
    }
}
```

### **External API Service**
```csharp
public class ExternalMovieApiService : IExternalMovieApiService
{
    private readonly HttpClient _httpClient;
    
    public async Task<IEnumerable<Movie>> GetMoviesFromProviderAsync(string provider)
    {
        // Implementation with resilience patterns
        // Circuit breaker, retry, timeout handling
    }
}
```

### **Database Context**
```csharp
public class MovieDbContext : DbContext
{
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<MoviePrice> MoviePrices { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations with proper relationships
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasMany(e => e.MoviePrices)
                  .WithOne(e => e.Movie)
                  .HasForeignKey(e => e.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

## 🌐 Presentation Layer

### **Clean Controllers**
```csharp
[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IGetMoviesWithPricesUseCase _getMoviesWithPricesUseCase;
    
    [HttpGet]
    public async Task<IActionResult> GetMovies()
    {
        try
        {
            var movies = await _getMoviesWithPricesUseCase.ExecuteAsync();
            return Ok(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movies");
            return StatusCode(500, new { error = "An error occurred while retrieving movies" });
        }
    }
}
```

## 🔌 Dependency Injection

### **Clean Architecture DI Registration**
```csharp
// Program.cs - Proper layer registration
// Application layer
builder.Services.AddScoped<IGetMoviesWithPricesUseCase, GetMoviesWithPricesUseCase>();
builder.Services.AddScoped<IGetMovieDetailUseCase, GetMovieDetailUseCase>();

// Infrastructure layer
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IExternalMovieApiService, ExternalMovieApiService>();
```

## 🧪 Testing Strategy

### **Unit Testing by Layer**

#### **Domain Layer Tests**
```csharp
[Test]
public void Movie_GetCheapestPrice_ReturnsLowestPrice()
{
    // Arrange
    var movie = new Movie("Test Movie");
    movie.AddPrice(new MoviePrice(1, "Provider1", 15.99m));
    movie.AddPrice(new MoviePrice(1, "Provider2", 12.99m));
    
    // Act
    var cheapest = movie.GetCheapestPrice();
    
    // Assert
    Assert.That(cheapest.Price, Is.EqualTo(12.99m));
    Assert.That(cheapest.Provider, Is.EqualTo("Provider2"));
}
```

#### **Application Layer Tests**
```csharp
[Test]
public async Task GetMoviesWithPricesUseCase_ReturnsMoviesWithPrices()
{
    // Arrange
    var mockRepository = new Mock<IMovieRepository>();
    var useCase = new GetMoviesWithPricesUseCase(mockRepository.Object);
    
    // Act & Assert
    var result = await useCase.ExecuteAsync();
    
    Assert.That(result, Is.Not.Null);
}
```

#### **Integration Tests**
```csharp
[Test]
public async Task MoviesController_GetMovies_ReturnsOkResult()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/movies");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.That(content, Is.Not.Empty);
}
```

## 🚀 API Endpoints

### **Movies API**

#### **GET /api/movies**
Get all movies with price comparison
```json
{
  "id": 1,
  "title": "The Matrix",
  "year": "1999",
  "genre": "Action, Sci-Fi",
  "prices": [
    {
      "provider": "Cinemaworld",
      "price": 15.99,
      "currency": "AUD",
      "isAvailable": true,
      "lastUpdated": "2024-01-01T10:00:00Z"
    }
  ],
  "cheapestPrice": {
    "provider": "Filmworld",
    "price": 14.99,
    "currency": "AUD"
  }
}
```

#### **GET /api/movies/{id}**
Get detailed movie information
```json
{
  "id": 1,
  "title": "The Matrix",
  "year": "1999",
  "type": "movie",
  "rated": "R",
  "released": "31 Mar 1999",
  "runtime": "136 min",
  "genre": "Action, Sci-Fi",
  "director": "Lana Wachowski, Lilly Wachowski",
  "plot": "A computer hacker learns...",
  "prices": [...],
  "cheapestPrice": {...}
}
```

#### **POST /api/refresh**
Refresh movie data from external APIs
```json
{
  "message": "Movie data refreshed successfully"
}
```

#### **GET /health**
System health check endpoint with environment information
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T10:00:00Z",
  "environment": "LOCALDEV"
}
```

#### **GET /api/providers**
Get current API provider configurations
```json
[
  {
    "name": "Cinemaworld",
    "baseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
    "isEnabled": true,
    "lastUpdated": "2024-01-01T10:00:00Z"
  }
]
```

#### **POST /api/providers/refresh**
Refresh API provider cache
```json
{
  "message": "API providers cache refreshed",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

## 🔧 Configuration

### **appsettings.json**
```json
{
  "Environment": "Production",
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=MoviePriceDB;...",
    "Redis": "localhost:6379"
  },
  "ExternalApi": {
    "CinemaworldToken": "your-token",
    "FilmworldToken": "your-token"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **Development Configuration**

#### With Dev Container (Recommended)
```json
{
  "Environment": "LOCALDEV",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MoviePriceDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;",
    "Redis": "your-redis-connection-string"
  },
  "ApiProviderService": {
    "ConfigurationServiceUrl": "http://localhost:5091/api/MockConfiguration/api-providers"
  },
  "ExternalApis": {
    "ApiToken": "your-development-token",
    "CinemaworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
    "FilmworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/filmworld"
  }
}
```

#### Without Dev Container
```json
{
  "Environment": "Development",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MoviePriceDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "Redis": "your-redis-connection-string"
  },
  "ApiProviderService": {
    "ConfigurationServiceUrl": "https://your-config-service.com/api/providers"
  },
  "ExternalApis": {
    "ApiToken": "your-development-token",
    "CinemaworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
    "FilmworldBaseUrl": "https://webjetapitest.azurewebsites.net/api/filmworld"
  }
}
```

## 🏃‍♂️ Running the API

### **Development**
```bash
# Restore packages
dotnet restore

# Run the API
dotnet run

# API will be available at:
# https://localhost:5091
# Swagger UI: https://localhost:5091/swagger
```

### **Docker**
```bash
# Build image
docker build -t MoviePriceComparison .

# Run container
docker run -p 5091:8080 MoviePriceComparison
```

### **Testing**
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔍 Monitoring & Observability

### **Health Checks**
- **API Health**: `/health` endpoint
- **Database Health**: EF Core health checks
- **External API Health**: Provider availability checks

### **Logging**
- **Structured logging** with Serilog
- **Request/Response logging** for debugging
- **Error logging** with correlation IDs

### **Metrics**
- **Performance counters** for API endpoints
- **External API response times**
- **Database query performance**

## 🛡️ Security Features

### **API Security**
- **HTTPS enforcement** in production
- **CORS configuration** for frontend
- **Rate limiting** to prevent abuse
- **Input validation** and sanitization

### **Secret Management**
- **Azure Key Vault** integration for production
- **Environment variables** for development
- **No hardcoded secrets** in code

## 🎯 Clean Architecture Benefits Achieved

### **✅ Testability**
- **Pure domain logic** easily unit testable
- **Mocked dependencies** for isolated testing
- **Integration tests** for end-to-end scenarios

### **✅ Maintainability**
- **Clear separation** of concerns
- **Single responsibility** principle
- **Loose coupling** between layers

### **✅ Flexibility**
- **Database independence** (LocalDB ↔ SQL Server ↔ Azure SQL)
- **External API independence** (easy to swap providers)
- **Framework independence** (core logic doesn't depend on ASP.NET)

### **✅ Business Logic Protection**
- **Domain entities** encapsulate business rules
- **Rich models** with behavior, not anemic data structures
- **Invariant enforcement** at the domain level

This Clean Architecture implementation provides a solid foundation for a maintainable, testable, and scalable movie price comparison API.
