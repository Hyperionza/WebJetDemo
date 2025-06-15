using System.Text.Json;
using MoviePriceComparison.Domain.Services;


namespace MoviePriceComparison.Infrastructure.Services
{
    public class ExternalMovieApiService : IExternalMovieApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalMovieApiService> _logger;
        private readonly IApiProviderService _movieProdiverService;

        public ExternalMovieApiService(
            IApiProviderService apiProviderService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalMovieApiService> logger)
        {
            _movieProdiverService = apiProviderService ?? throw new ArgumentNullException(nameof(apiProviderService));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ExternalMovieSummaryDto>> GetMoviesFromProviderAsync(string providerId)
        {
            try
            {
                var provider = await _movieProdiverService.GetApiProviderAsync(providerId);
                if (provider == null)
                {
                    _logger.LogWarning("Unknown provider: {ProviderId}", providerId);
                    return Enumerable.Empty<ExternalMovieSummaryDto>();
                }

                if (string.IsNullOrEmpty(provider.ApiToken))
                {
                    _logger.LogWarning("No API token configured for provider: {ProviderId}", providerId);
                    return Enumerable.Empty<ExternalMovieSummaryDto>();
                }
                var renderedUrlFragment = provider.Endpoints.Movies;
                if (renderedUrlFragment.StartsWith('/')) renderedUrlFragment = renderedUrlFragment.Substring(1);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{provider.BaseUrl}/{renderedUrlFragment}");
                request.Headers.Add("x-access-token", provider.ApiToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get movies from {ProviderId}. Status: {StatusCode}",
                        providerId, response.StatusCode);
                    return Enumerable.Empty<ExternalMovieSummaryDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var movieDtos = JsonSerializer.Deserialize<ExternalMovieSummaryDto[]>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return movieDtos ?? Enumerable.Empty<ExternalMovieSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies from provider {ProviderId}", providerId);
                return Enumerable.Empty<ExternalMovieSummaryDto>();
            }
        }

        public async Task<ExternalMovieDetailDto?> GetMovieDetailsFromProviderAsync(string providerId, string movieId)
        {
            try
            {
                var provider = await _movieProdiverService.GetApiProviderAsync(providerId);
                if (provider == null)
                {
                    _logger.LogWarning("Unknown provider: {ProviderId}", providerId);
                    return null;
                }

                if (string.IsNullOrEmpty(provider.ApiToken))
                {
                    _logger.LogWarning("No API token configured for provider: {ProviderId}", providerId);
                    return null;
                }

                var renderedUrlFragment = provider.Endpoints.MovieDetail.Replace("{id}", movieId);
                if (renderedUrlFragment.StartsWith('/')) renderedUrlFragment = renderedUrlFragment.Substring(1);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{provider.BaseUrl}/{renderedUrlFragment}");
                request.Headers.Add("x-access-token", provider.ApiToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get movie details from {ProviderId} for movie {MovieId}. Status: {StatusCode}",
                        providerId, movieId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var movieDto = JsonSerializer.Deserialize<ExternalMovieDetailDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return movieDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details from provider {ProviderId} for movie {MovieId}", providerId, movieId);
                return null;
            }
        }
    }

    // DTOs for external API responses
    public class ExternalMovieSummaryDto
    {
        public string ID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
    }

    public class ExternalMovieDetailDto
    {
        public string ID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Rated { get; set; } = string.Empty;
        public string Released { get; set; } = string.Empty;
        public string Runtime { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Writer { get; set; } = string.Empty;
        public string Actors { get; set; } = string.Empty;
        public string Plot { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Awards { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public string Metascore { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Votes { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
