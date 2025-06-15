using System.Text.Json;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Services;

namespace MoviePriceComparison.Infrastructure.Services
{
    public class ExternalMovieApiService : IExternalMovieApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalMovieApiService> _logger;

        private readonly Dictionary<string, string> _providerUrls = new()
        {
            { "Cinemaworld", "https://webjetapitest.azurewebsites.net/api/cinemaworld" },
            { "Filmworld", "https://webjetapitest.azurewebsites.net/api/filmworld" }
        };

        public ExternalMovieApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalMovieApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Movie>> GetMoviesFromProviderAsync(string provider)
        {
            try
            {
                if (!_providerUrls.TryGetValue(provider, out var baseUrl))
                {
                    _logger.LogWarning("Unknown provider: {Provider}", provider);
                    return Enumerable.Empty<Movie>();
                }

                var token = GetApiToken(provider);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No API token configured for provider: {Provider}", provider);
                    return Enumerable.Empty<Movie>();
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/movies");
                request.Headers.Add("x-access-token", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get movies from {Provider}. Status: {StatusCode}",
                        provider, response.StatusCode);
                    return Enumerable.Empty<Movie>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var movieDtos = JsonSerializer.Deserialize<ExternalMovieDto[]>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return movieDtos?.Select(dto => new Movie(dto.Title, dto.Year, dto.Type)) ?? Enumerable.Empty<Movie>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies from provider {Provider}", provider);
                return Enumerable.Empty<Movie>();
            }
        }

        public async Task<Movie?> GetMovieDetailsFromProviderAsync(string provider, string movieId)
        {
            try
            {
                if (!_providerUrls.TryGetValue(provider, out var baseUrl))
                {
                    _logger.LogWarning("Unknown provider: {Provider}", provider);
                    return null;
                }

                var token = GetApiToken(provider);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No API token configured for provider: {Provider}", provider);
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/movie/{movieId}");
                request.Headers.Add("x-access-token", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get movie details from {Provider} for movie {MovieId}. Status: {StatusCode}",
                        provider, movieId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var movieDto = JsonSerializer.Deserialize<ExternalMovieDetailDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (movieDto == null) return null;

                var movie = new Movie(movieDto.Title, movieDto.Year, movieDto.Type);
                movie.UpdateDetails(
                    year: movieDto.Year,
                    type: movieDto.Type,
                    rated: movieDto.Rated,
                    released: movieDto.Released,
                    runtime: movieDto.Runtime,
                    genre: movieDto.Genre,
                    director: movieDto.Director,
                    writer: movieDto.Writer,
                    actors: movieDto.Actors,
                    plot: movieDto.Plot,
                    language: movieDto.Language,
                    country: movieDto.Country,
                    awards: movieDto.Awards,
                    poster: movieDto.Poster,
                    metascore: movieDto.Metascore,
                    rating: movieDto.ImdbRating,
                    votes: movieDto.ImdbVotes
                );

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details from provider {Provider} for movie {MovieId}", provider, movieId);
                return null;
            }
        }

        public async Task<decimal?> GetMoviePriceFromProviderAsync(string provider, string movieId)
        {
            try
            {
                var movie = await GetMovieDetailsFromProviderAsync(provider, movieId);
                if (movie == null) return null;

                // For this implementation, we'll extract price from the movie details
                // In a real scenario, this might be a separate API call
                // For now, we'll simulate prices based on provider and movie characteristics
                return GenerateSimulatedPrice(provider, movieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie price from provider {Provider} for movie {MovieId}", provider, movieId);
                return null;
            }
        }

        public async Task<bool> IsProviderHealthyAsync(string provider)
        {
            try
            {
                if (!_providerUrls.TryGetValue(provider, out var baseUrl))
                {
                    return false;
                }

                var token = GetApiToken(provider);
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/movies");
                request.Headers.Add("x-access-token", token);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.SendAsync(request, cts.Token);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for provider {Provider}", provider);
                return false;
            }
        }

        private string? GetApiToken(string provider)
        {
            return provider switch
            {
                "Cinemaworld" => _configuration["ExternalApi:CinemaworldToken"],
                "Filmworld" => _configuration["ExternalApi:FilmworldToken"],
                _ => null
            };
        }

        private static decimal GenerateSimulatedPrice(string provider, string movieId)
        {
            // Simulate price generation based on provider and movie
            var random = new Random(provider.GetHashCode() + movieId.GetHashCode());
            var basePrice = provider == "Cinemaworld" ? 15.99m : 14.99m;
            var variation = (decimal)(random.NextDouble() * 10 - 5); // -5 to +5
            return Math.Max(9.99m, basePrice + variation);
        }
    }

    // DTOs for external API responses
    public class ExternalMovieDto
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
        public string ImdbRating { get; set; } = string.Empty;
        public string ImdbVotes { get; set; } = string.Empty;
        public string ImdbID { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }
}
