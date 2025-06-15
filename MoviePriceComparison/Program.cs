using Microsoft.EntityFrameworkCore;
using MoviePriceComparison.Infrastructure.Data;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Domain.Services;
using MoviePriceComparison.Infrastructure.Repositories;
using MoviePriceComparison.Infrastructure.Services;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Services;
using MoviePriceComparison.Models;

var builder = WebApplication.CreateBuilder(args);

// Determine environment
var environment = builder.Configuration["Environment"] ?? "Production";
var isLocalDev = environment == "LOCALDEV";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
if (isLocalDev)
{
    builder.Services.AddDbContext<MovieDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         "Data Source=movies.db"));
}
else
{
    builder.Services.AddDbContext<MovieDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure caching
builder.Services.AddMemoryCache(); // For API provider caching

if (isLocalDev)
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });
}

// Configure API Provider Service
builder.Services.Configure<ApiProviderConfiguration>(options =>
{
    // Set the configuration service URL to our own mock endpoint for demo purposes
    // In production, this would point to an external configuration service
    options.ConfigurationServiceUrl = isLocalDev
        ? "http://localhost:5091/api/MockConfiguration/api-providers"
        : builder.Configuration["ApiProviderService:ConfigurationServiceUrl"] ?? "";
    options.CacheDurationMinutes = 15;
    options.TimeoutSeconds = 30;
});

// Configure HTTP clients
builder.Services.AddHttpClient<IExternalMovieApiService, ExternalMovieApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IApiProviderService, ApiProviderService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Clean Architecture layers
// Domain layer (no dependencies)
// Application layer
builder.Services.AddScoped<IGetMoviesWithPricesUseCase, GetMoviesWithPricesUseCase>();
builder.Services.AddScoped<IGetMovieDetailUseCase, GetMovieDetailUseCase>();

// Infrastructure layer
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IExternalMovieApiService, ExternalMovieApiService>();

// Register new dynamic API provider services
builder.Services.AddScoped<IApiProviderService, ApiProviderService>();

// Legacy services (keeping for backward compatibility)
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
builder.Services.AddScoped<IMovieService, MovieService>();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001", "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error initializing database");
    }
}

// Add a simple health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = environment
});

// Add endpoint to refresh API provider cache
app.MapPost("/api/providers/refresh", async (IApiProviderService apiProviderService) =>
{
    await apiProviderService.RefreshApiProvidersAsync();
    return Results.Ok(new { message = "API providers cache refreshed", timestamp = DateTime.UtcNow });
});

// Add endpoint to get current API providers
app.MapGet("/api/providers", async (IApiProviderService apiProviderService) =>
{
    var providers = await apiProviderService.GetApiProvidersAsync();
    return Results.Ok(providers);
});

app.Run();
