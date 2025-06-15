using System.Text.Json;
using MoviePriceComparison.DTOs;
using MoviePriceComparison.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace MoviePriceComparison.Services
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IApiProviderService _apiProviderService;
        private readonly Dictionary<string, string> _fallbackApiTokens = new();

        public ExternalApiService(
            HttpClient httpClient,
            ILogger<ExternalApiService> logger,
            IConfiguration configuration,
            IApiProviderService apiProviderService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _apiProviderService = apiProviderService;

            // Initialize fallback API tokens for legacy support
            InitializeFallbackApiTokensAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeFallbackApiTokensAsync()
        {
            try
            {
                var environment = _configuration["Environment"];

                if (environment == "LOCALDEV")
                {
                    // For local development, use the token directly from configuration
                    var token = _configuration["ExternalApis:ApiToken"] ?? "sjd1HfkjU83ksdsm3802k";
                    _fallbackApiTokens["cinemaworld"] = token;
                    _fallbackApiTokens["filmworld"] = token;
                    _logger.LogInformation("Using local development fallback API tokens");
                }
                else
                {
                    // For production, get tokens from Azure Key Vault as fallback
                    var keyVaultUrl = _configuration["KeyVault:Url"];
                    if (!string.IsNullOrEmpty(keyVaultUrl))
                    {
                        var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

                        var cinemaWorldSecret = await client.GetSecretAsync("cinemaworld-api-token");
                        var filmWorldSecret = await client.GetSecretAsync("filmworld-api-token");

                        _fallbackApiTokens["cinemaworld"] = cinemaWorldSecret.Value.Value;
                        _fallbackApiTokens["filmworld"] = filmWorldSecret.Value.Value;

                        _logger.LogInformation("Successfully retrieved fallback API tokens from Key Vault");
                    }
                    else
                    {
                        _logger.LogWarning("Key Vault URL not configured, using default fallback token");
                        var token = "sjd1HfkjU83ksdsm3802k";
                        _fallbackApiTokens["cinemaworld"] = token;
                        _fallbackApiTokens["filmworld"] = token;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize fallback API tokens, using default");
                var token = "sjd1HfkjU83ksdsm3802k";
                _fallbackApiTokens["cinemaworld"] = token;
                _fallbackApiTokens["filmworld"] = token;
            }
        }

        public async Task<MoviesListResponse?> GetMoviesAsync(string provider)
        {
            try
            {
                // Get provider configuration dynamically
                var apiProvider = await _apiProviderService.GetApiProviderAsync(provider);

                if (apiProvider == null || !apiProvider.IsEnabled)
                {
                    _logger.LogWarning("Provider {Provider} is not available or disabled", provider);
                    return null;
                }

                var url = $"{apiProvider.BaseUrl.TrimEnd('/')}{apiProvider.Endpoints.Movies}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Add headers from provider configuration
                foreach (var header in apiProvider.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Set timeout from provider configuration
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(apiProvider.TimeoutSeconds));

                _logger.LogInformation("Fetching movies from {Provider}: {Url}", provider, url);

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<MoviesListResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully fetched {Count} movies from {Provider}",
                        result?.Movies?.Count ?? 0, provider);

                    return result;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch movies from {Provider}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        provider, response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movies from {Provider}", provider);
                return await GetMoviesWithFallbackAsync(provider);
            }
        }

        public async Task<MovieDetailDto?> GetMovieDetailAsync(string provider, string movieId)
        {
            try
            {
                // Get provider configuration dynamically
                var apiProvider = await _apiProviderService.GetApiProviderAsync(provider);

                if (apiProvider == null || !apiProvider.IsEnabled)
                {
                    _logger.LogWarning("Provider {Provider} is not available or disabled", provider);
                    return null;
                }

                var url = $"{apiProvider.BaseUrl.TrimEnd('/')}{apiProvider.Endpoints.MovieDetail.Replace("{id}", movieId)}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Add headers from provider configuration
                foreach (var header in apiProvider.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Set timeout from provider configuration
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(apiProvider.TimeoutSeconds));

                _logger.LogInformation("Fetching movie detail from {Provider}: {Url}", provider, url);

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<MovieDetailDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully fetched movie detail for {MovieId} from {Provider}",
                        movieId, provider);

                    return result;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch movie detail for {MovieId} from {Provider}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        movieId, provider, response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie detail for {MovieId} from {Provider}", movieId, provider);
                return await GetMovieDetailWithFallbackAsync(provider, movieId);
            }
        }

        public async Task<bool> IsHealthyAsync(string provider)
        {
            try
            {
                // Get provider configuration dynamically
                var apiProvider = await _apiProviderService.GetApiProviderAsync(provider);

                if (apiProvider == null || !apiProvider.IsEnabled)
                {
                    return false;
                }

                var url = $"{apiProvider.BaseUrl.TrimEnd('/')}{apiProvider.Endpoints.Health}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Add headers from provider configuration
                foreach (var header in apiProvider.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Set a shorter timeout for health checks
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.SendAsync(request, cts.Token);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for {Provider}", provider);
                return false;
            }
        }

        // Fallback methods for when dynamic configuration fails
        private async Task<MoviesListResponse?> GetMoviesWithFallbackAsync(string provider)
        {
            try
            {
                _logger.LogInformation("Using fallback configuration for {Provider}", provider);

                var baseUrl = "https://webjetapitest.azurewebsites.net/api";
                var url = $"{baseUrl}/{provider.ToLower()}/movies";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (_fallbackApiTokens.TryGetValue(provider.ToLower(), out var token))
                {
                    request.Headers.Add("x-access-token", token);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MoviesListResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback also failed for {Provider}", provider);
                return null;
            }
        }

        private async Task<MovieDetailDto?> GetMovieDetailWithFallbackAsync(string provider, string movieId)
        {
            try
            {
                _logger.LogInformation("Using fallback configuration for {Provider} movie detail", provider);

                var baseUrl = "https://webjetapitest.azurewebsites.net/api";
                var url = $"{baseUrl}/{provider.ToLower()}/movie/{movieId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (_fallbackApiTokens.TryGetValue(provider.ToLower(), out var token))
                {
                    request.Headers.Add("x-access-token", token);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MovieDetailDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback also failed for {Provider} movie detail", provider);
                return null;
            }
        }
    }
}
