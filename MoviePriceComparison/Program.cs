using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MoviePriceComparison.Domain.Services;
using MoviePriceComparison.Infrastructure.Services;
using MoviePriceComparison.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Determine environment
var environment = builder.Configuration["Environment"] ?? "Production";
var isLocalDev = environment == "LOCALDEV";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure caching
builder.Services.AddMemoryCache(); // For API provider caching

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
}
else
{
    // Fallback to in-memory cache if Redis is not available (non-container development)
    builder.Services.AddDistributedMemoryCache();
}

// Configure 3rd party movie provider api administration service
builder.Services.Configure<ApiProviderConfiguration>(options =>
{
    // Set the 3rd party movie provider api administration service URL to our own mock endpoint for demo purposes
    // In production, this would point to an external microservice
    options.ApiProviderServiceUrl = builder.Configuration["ApiProviderService:ApiProviderServiceUrl"] ?? "";
    options.CacheDurationMinutes = builder.Configuration.GetValue<int>("ApiProviderService:CacheDurationMinutes", 15);
    options.TimeoutSeconds = builder.Configuration.GetValue<int>("ApiProviderService:TimeoutSeconds", 30);
});

// Configure external movie API cache settings
builder.Services.Configure<MoviePriceComparison.Infrastructure.Repositories.ExternalMovieApiCacheSettings>(
    builder.Configuration.GetSection("ExternalMovieApiCacheSettings"));

// Configure HTTP clients
builder.Services.AddHttpClient<IExternalMovieApiService, ExternalMovieApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IApiProviderService, ApiProviderService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add HttpClient for poster URL validation
builder.Services.AddHttpClient<GetMovieDetailUseCase>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10); // Shorter timeout for image validation
});

// Register Clean Architecture layers
// Domain layer (no dependencies)
// Application layer
builder.Services.AddScoped<IGetMoviesWithPricesUseCase, GetMoviesWithPricesUseCase>();
builder.Services.AddScoped<IGetMovieDetailUseCase, GetMovieDetailUseCase>();

// Infrastructure layer
builder.Services.AddScoped<IExternalMovieApiService, ExternalMovieApiService>();
builder.Services.AddScoped<MoviePriceComparison.Domain.Repositories.IMovieRepository, MoviePriceComparison.Infrastructure.Repositories.MovieRepository>();

// Register new dynamic API provider services
builder.Services.AddScoped<IApiProviderService, ApiProviderService>();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
        //policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001", "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
              //.AllowCredintials();
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
