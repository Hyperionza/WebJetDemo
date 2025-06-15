using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Services;
using System.Text.Json;

namespace MoviePriceComparison.Infrastructure.Services
{
    /// <summary>
    /// This is a mock service that would connect to another 
    /// microservice which administers 3rd party movie providers APIs
    /// </summary>
    public class ApiProviderService : IApiProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApiProviderService> _logger;
        private readonly ApiProviderConfiguration _config;
        private const string CACHE_KEY = "api_providers";

        public ApiProviderService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<ApiProviderService> logger,
            IOptions<ApiProviderConfiguration> config)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<List<ApiProvider>> GetApiProvidersAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<ApiProvider>? cachedProviders) && cachedProviders != null)
            {
                _logger.LogDebug("Returning cached API providers");
                return cachedProviders;
            }

            try
            {
                _logger.LogInformation("Fetching API providers from configuration service");
                var providers = await FetchApiProvidersFromServiceAsync();

                // Cache the providers
                _cache.Set(CACHE_KEY, providers, TimeSpan.FromMinutes(_config.CacheDurationMinutes));

                return providers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch API providers from service, falling back to default configuration");
                return GetFallbackProviders();
            }
        }

        public async Task<ApiProvider?> GetApiProviderAsync(string providerId)
        {
            var providers = await GetApiProvidersAsync();
            return providers.FirstOrDefault(p => p.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task RefreshApiProvidersAsync()
        {
            _logger.LogInformation("Refreshing API providers cache");
            _cache.Remove(CACHE_KEY);
            await GetApiProvidersAsync();
        }

        public async Task<bool> IsProviderEnabledAsync(string providerId)
        {
            var provider = await GetApiProviderAsync(providerId);
            return provider?.IsEnabled ?? false;
        }

        private async Task<List<ApiProvider>> FetchApiProvidersFromServiceAsync()
        {
            // maintain the async pattern
            await Task.CompletedTask;
            // mock the response
            return GetFallbackProviders();

            // What it might otherwise look like if there was a real microservice to fetch from
            // try
            // {
            //     var response = await _httpClient.GetAsync(_config.ApiProviderServiceUrl);
            //     response.EnsureSuccessStatusCode();

            //     var json = await response.Content.ReadAsStringAsync();
            //     var apiResponse = JsonSerializer.Deserialize<ApiProvidersResponse>(json, new JsonSerializerOptions
            //     {
            //         PropertyNameCaseInsensitive = true
            //     });

            //     if (apiResponse?.Providers == null || !apiResponse.Providers.Any())
            //     {
            //         _logger.LogWarning("No providers returned from configuration service");
            //         return GetFallbackProviders();
            //     }

            //     _logger.LogInformation("Successfully fetched {Count} API providers from configuration service",
            //         apiResponse.Providers.Count);

            //     return apiResponse.Providers;
            // }
            // catch (HttpRequestException ex)
            // {
            //     _logger.LogError(ex, "HTTP error while fetching API providers from {Url}", _config.ApiProviderServiceUrl);
            //     throw;
            // }
            // catch (JsonException ex)
            // {
            //     _logger.LogError(ex, "JSON parsing error while processing API providers response");
            //     throw;
            // }
        }

        private List<ApiProvider> GetFallbackProviders()
        {
            _logger.LogInformation("Using fallback API providers configuration");

            return new List<ApiProvider>
            {
                new ApiProvider
                {
                    Id = "cinemaworld",
                    Name = "cinemaworld",
                    DisplayName = "Cinemaworld",
                    BaseUrl = "https://webjetapitest.azurewebsites.net/api/cinemaworld",
                    ApiToken = "sjd1HfkjU83ksdsm3802k",
                    IsEnabled = true,
                    Priority = 1,
                    TimeoutSeconds = 30,
                    Endpoints = new ApiEndpoints
                    {
                        Movies = "/movies",
                        MovieDetail = "/movie/{id}"
                    }
                },
                new ApiProvider
                {
                    Id = "filmworld",
                    Name = "filmworld",
                    DisplayName = "Filmworld",
                    BaseUrl = "https://webjetapitest.azurewebsites.net/api/filmworld",
                    ApiToken = "sjd1HfkjU83ksdsm3802k",
                    IsEnabled = true,
                    Priority = 2,
                    TimeoutSeconds = 30,
                    Endpoints = new ApiEndpoints
                    {
                        Movies = "/movies",
                        MovieDetail = "/movie/{id}"
                    }
                }
            };
        }
    }

    public class ApiProviderConfiguration
    {
        public string ApiProviderServiceUrl { get; set; } = string.Empty;
        public int CacheDurationMinutes { get; set; } = 15;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
