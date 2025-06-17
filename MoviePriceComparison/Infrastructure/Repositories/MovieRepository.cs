using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MoviePriceComparison.Domain.Services;
using System.Diagnostics.Contracts;
using MoviePriceComparison.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace MoviePriceComparison.Infrastructure.Repositories
{
    public class ExternalMovieApiCacheSettings
    {
        public int CacheDurationMinutes { get; set; } = 5;
        public int MaxAgeMinutes { get; set; } = 10;
    }

    public class MovieRepository : IMovieRepository
    {
        private readonly IMemoryCache _cache;
        private readonly IExternalMovieApiService _externalMovieApiService;
        private readonly IApiProviderService _apiProviderService;
        private readonly ILogger<MovieRepository> _logger;
        private readonly ExternalMovieApiCacheSettings _cacheSettings;

        private const string LIST_CACHE_KEY = "movies_list";
        //private const string DETAIL_CACHE_KEY = "movies_detail";

        public MovieRepository(IMemoryCache cache,
        IExternalMovieApiService externalMovieApiService,
        IApiProviderService apiProviderService,
        ILogger<MovieRepository> logger,
        IOptions<ExternalMovieApiCacheSettings> cacheSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _externalMovieApiService = externalMovieApiService ?? throw new ArgumentNullException(nameof(externalMovieApiService));
            _apiProviderService = apiProviderService ?? throw new ArgumentNullException(nameof(apiProviderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheSettings = cacheSettings?.Value ?? throw new ArgumentNullException(nameof(cacheSettings));
        }

        public async Task RefreshData()
        {
            _logger.LogInformation("Refreshing Movies cache");
            _cache.Remove(LIST_CACHE_KEY);
            //_cache.Remove(DETAIL_CACHE_KEY);
            await _apiProviderService.RefreshApiProvidersAsync(); // might not be necessary
            await GetAllAsync();
        }

        // having dificulty isolating when something is stale and when its expired
        // gone with the simplest option for MVP.
        public async Task<IEnumerable<MovieSummary>> GetAllAsync()
        {
            // Check cache first
            if (_cache.TryGetValue(LIST_CACHE_KEY, out IEnumerable<MovieSummary>? cachedMovies))
            {
                _logger.LogInformation("Returning cached movies list");
                return cachedMovies!;
            }

            _logger.LogInformation("Cache miss - fetching movies from external APIs");

            var result = new List<MovieSummary>();
            var providers = await _apiProviderService.GetApiProvidersAsync();

            // Process providers sequentially to avoid race conditions
            foreach (var provider in providers.Where(p => p.IsEnabled))
            {
                try
                {
                    var movies = await _externalMovieApiService.GetMoviesFromProviderAsync(provider.Id);

                    foreach (var movieItem in movies)
                    {
                        var externalSpecifics = await _externalMovieApiService.GetMovieDetailsFromProviderAsync(provider.Id, movieItem.ID);
                        var currentMovie = result.FirstOrDefault(x => x.Title == movieItem.Title);

                        if (currentMovie == null)
                        {
                            currentMovie = new MovieSummary { Title = movieItem.Title };
                            result.Add(currentMovie);
                        }

                        // Update movie properties with external data
                        currentMovie.Actors = externalSpecifics?.Actors ?? currentMovie.Actors;
                        currentMovie.Awards = externalSpecifics?.Awards ?? currentMovie.Awards;
                        currentMovie.Country = externalSpecifics?.Country ?? currentMovie.Country;
                        currentMovie.Director = externalSpecifics?.Director ?? currentMovie.Director;
                        currentMovie.Genre = externalSpecifics?.Genre ?? currentMovie.Genre;
                        currentMovie.Language = externalSpecifics?.Language ?? currentMovie.Language;
                        currentMovie.Metascore = externalSpecifics?.Metascore ?? currentMovie.Metascore;
                        currentMovie.Plot = externalSpecifics?.Plot ?? currentMovie.Plot;
                        currentMovie.Rated = externalSpecifics?.Rated ?? currentMovie.Rated;
                        currentMovie.Rating = externalSpecifics?.Rating ?? currentMovie.Rating;
                        currentMovie.Released = externalSpecifics?.Released ?? currentMovie.Released;
                        currentMovie.Runtime = externalSpecifics?.Runtime ?? currentMovie.Runtime;
                        currentMovie.Type = externalSpecifics?.Type ?? movieItem.Type ?? currentMovie.Type;
                        currentMovie.Votes = externalSpecifics?.Votes ?? currentMovie.Votes;
                        currentMovie.Writer = externalSpecifics?.Writer ?? currentMovie.Writer;
                        currentMovie.Year = externalSpecifics?.Year ?? movieItem.Year ?? currentMovie.Year;
                        currentMovie.UpdatedAt = DateTime.UtcNow;

                        if (externalSpecifics != null)
                        {
                            var currentSpecifics = currentMovie.ProviderSpecificDetails.FirstOrDefault(x => x.ProviderId == provider.Id);
                            if (currentSpecifics == null)
                            {
                                currentSpecifics = new MovieProviderDetail
                                {
                                    Provider = provider.DisplayName,
                                    ProviderId = provider.Id,
                                    MovieId = movieItem.ID
                                };
                                currentMovie.ProviderSpecificDetails.Add(currentSpecifics);
                            }

                            currentSpecifics.PosterUrl = externalSpecifics?.Poster ?? currentSpecifics.PosterUrl;
                            if (externalSpecifics?.Price != null && decimal.TryParse(externalSpecifics.Price, out decimal dPrice))
                            {
                                currentSpecifics.Price = dPrice;
                            }
                            currentSpecifics.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching movies from provider {ProviderId}", provider.Id);
                    // Continue with other providers even if one fails
                }
            }

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.CacheDurationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(_cacheSettings.MaxAgeMinutes / 2),
                Priority = CacheItemPriority.High
            };

            _cache.Set(LIST_CACHE_KEY, result, cacheOptions);
            _logger.LogInformation("Cached {MovieCount} movies for {CacheDuration} minutes", result.Count, _cacheSettings.CacheDurationMinutes);

            return result;
        }
    }
}
