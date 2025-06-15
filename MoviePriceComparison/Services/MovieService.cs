using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using MoviePriceComparison.Data;
using MoviePriceComparison.DTOs;
using MoviePriceComparison.Models;

namespace MoviePriceComparison.Services
{
    public class MovieService : IMovieService
    {
        private readonly MovieDbContext _context;
        private readonly IExternalApiService _externalApiService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<MovieService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
        private readonly string[] _providers = { "cinemaworld", "filmworld" };

        public MovieService(
            MovieDbContext context,
            IExternalApiService externalApiService,
            IDistributedCache cache,
            ILogger<MovieService> logger)
        {
            _context = context;
            _externalApiService = externalApiService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<MovieComparisonDto>> GetMoviesWithPricesAsync()
        {
            try
            {
                const string cacheKey = "movies_with_prices";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cached = JsonSerializer.Deserialize<List<MovieComparisonDto>>(cachedData);
                    if (cached != null)
                    {
                        _logger.LogInformation("Returning cached movies data");
                        return cached;
                    }
                }

                // Refresh data if not in cache or cache is invalid
                await RefreshMovieDataAsync();

                // Get fresh data from database
                var movies = await _context.Movies
                    .Include(m => m.MoviePrices)
                    .OrderBy(m => m.Title)
                    .ToListAsync();

                var result = movies.Select(MapToMovieComparisonDto).ToList();

                // Cache the result
                var serialized = JsonSerializer.Serialize(result);
                await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                });

                _logger.LogInformation("Returning {Count} movies with prices", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies with prices");
                return new List<MovieComparisonDto>();
            }
        }

        public async Task<MovieDetailResponseDto?> GetMovieDetailAsync(int movieId)
        {
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.MoviePrices)
                    .FirstOrDefaultAsync(m => m.Id == movieId);

                if (movie == null)
                {
                    return null;
                }

                return MapToMovieDetailResponseDto(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie detail for ID {MovieId}", movieId);
                return null;
            }
        }

        public async Task<List<MovieComparisonDto>> SearchMoviesAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return await GetMoviesWithPricesAsync();
                }

                var movies = await _context.Movies
                    .Include(m => m.MoviePrices)
                    .Where(m => m.Title.Contains(query) || 
                               (m.Genre != null && m.Genre.Contains(query)) ||
                               (m.Director != null && m.Director.Contains(query)) ||
                               (m.Actors != null && m.Actors.Contains(query)))
                    .OrderBy(m => m.Title)
                    .ToListAsync();

                var result = movies.Select(MapToMovieComparisonDto).ToList();
                _logger.LogInformation("Search for '{Query}' returned {Count} movies", query, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query '{Query}'", query);
                return new List<MovieComparisonDto>();
            }
        }

        public async Task RefreshMovieDataAsync()
        {
            _logger.LogInformation("Starting movie data refresh");

            foreach (var provider in _providers)
            {
                try
                {
                    await RefreshProviderDataAsync(provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing data for provider {Provider}", provider);
                }
            }

            // Clear cache to force fresh data on next request
            await _cache.RemoveAsync("movies_with_prices");
            
            _logger.LogInformation("Movie data refresh completed");
        }

        private async Task RefreshProviderDataAsync(string provider)
        {
            _logger.LogInformation("Refreshing data for provider {Provider}", provider);

            var moviesResponse = await _externalApiService.GetMoviesAsync(provider);
            if (moviesResponse?.Movies == null)
            {
                _logger.LogWarning("No movies data received from {Provider}", provider);
                return;
            }

            foreach (var movieSummary in moviesResponse.Movies)
            {
                try
                {
                    // Get detailed movie information including price
                    var movieDetail = await _externalApiService.GetMovieDetailAsync(provider, movieSummary.ID);
                    if (movieDetail == null)
                    {
                        _logger.LogWarning("Could not get detail for movie {MovieId} from {Provider}", movieSummary.ID, provider);
                        continue;
                    }

                    await UpsertMovieAsync(movieDetail, provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing movie {MovieId} from {Provider}", movieSummary.ID, provider);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Completed refreshing data for provider {Provider}", provider);
        }

        private async Task UpsertMovieAsync(MovieDetailDto movieDetail, string provider)
        {
            // Try to find existing movie by title and year
            var existingMovie = await _context.Movies
                .Include(m => m.MoviePrices)
                .FirstOrDefaultAsync(m => m.Title == movieDetail.Title && m.Year == movieDetail.Year);

            if (existingMovie == null)
            {
                // Create new movie
                existingMovie = new Movie
                {
                    Title = movieDetail.Title,
                    Year = movieDetail.Year,
                    Type = movieDetail.Type,
                    Rated = movieDetail.Rated,
                    Released = movieDetail.Released,
                    Runtime = movieDetail.Runtime,
                    Genre = movieDetail.Genre,
                    Director = movieDetail.Director,
                    Writer = movieDetail.Writer,
                    Actors = movieDetail.Actors,
                    Plot = movieDetail.Plot,
                    Language = movieDetail.Language,
                    Country = movieDetail.Country,
                    Awards = movieDetail.Awards,
                    Poster = movieDetail.Poster,
                    Metascore = movieDetail.Metascore,
                    Rating = movieDetail.Rating,
                    Votes = movieDetail.Votes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Movies.Add(existingMovie);
                await _context.SaveChangesAsync(); // Save to get the ID
            }
            else
            {
                // Update existing movie
                existingMovie.UpdatedAt = DateTime.UtcNow;
                // Update other fields if needed (could add logic to update only if data is newer)
            }

            // Upsert price information
            var existingPrice = existingMovie.MoviePrices.FirstOrDefault(p => p.Provider == provider);
            
            if (decimal.TryParse(movieDetail.Price, out var price))
            {
                var freshness = await DetermineFreshness(provider);
                
                if (existingPrice == null)
                {
                    existingPrice = new MoviePrice
                    {
                        MovieId = existingMovie.Id,
                        Provider = provider,
                        ExternalId = movieDetail.ID,
                        Price = price,
                        LastUpdated = DateTime.UtcNow,
                        Freshness = freshness
                    };
                    _context.MoviePrices.Add(existingPrice);
                }
                else
                {
                    existingPrice.Price = price;
                    existingPrice.LastUpdated = DateTime.UtcNow;
                    existingPrice.Freshness = freshness;
                    existingPrice.ExternalId = movieDetail.ID;
                }
            }
        }

        private async Task<DataFreshness> DetermineFreshness(string provider)
        {
            var isHealthy = await _externalApiService.IsHealthyAsync(provider);
            return isHealthy ? DataFreshness.Fresh : DataFreshness.Cached;
        }

        public async Task<List<ApiHealthDto>> GetApiHealthAsync()
        {
            var healthChecks = new List<ApiHealthDto>();

            foreach (var provider in _providers)
            {
                try
                {
                    var isHealthy = await _externalApiService.IsHealthyAsync(provider);
                    healthChecks.Add(new ApiHealthDto
                    {
                        Provider = provider,
                        IsHealthy = isHealthy,
                        LastChecked = DateTime.UtcNow,
                        ErrorMessage = isHealthy ? null : "API is not responding"
                    });
                }
                catch (Exception ex)
                {
                    healthChecks.Add(new ApiHealthDto
                    {
                        Provider = provider,
                        IsHealthy = false,
                        LastChecked = DateTime.UtcNow,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return healthChecks;
        }

        private static MovieComparisonDto MapToMovieComparisonDto(Movie movie)
        {
            var prices = movie.MoviePrices.Select(p => new PriceInfoDto
            {
                Provider = p.Provider,
                Price = p.Price,
                Freshness = p.Freshness,
                LastUpdated = p.LastUpdated
            }).ToList();

            var bestPrice = prices.OrderBy(p => p.Price).FirstOrDefault();

            return new MovieComparisonDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Genre = movie.Genre,
                Director = movie.Director,
                Poster = movie.Poster,
                Rating = movie.Rating,
                Prices = prices,
                BestPrice = bestPrice
            };
        }

        private static MovieDetailResponseDto MapToMovieDetailResponseDto(Movie movie)
        {
            var prices = movie.MoviePrices.Select(p => new PriceInfoDto
            {
                Provider = p.Provider,
                Price = p.Price,
                Freshness = p.Freshness,
                LastUpdated = p.LastUpdated
            }).ToList();

            var bestPrice = prices.OrderBy(p => p.Price).FirstOrDefault();

            return new MovieDetailResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Rated = movie.Rated,
                Released = movie.Released,
                Runtime = movie.Runtime,
                Genre = movie.Genre,
                Director = movie.Director,
                Writer = movie.Writer,
                Actors = movie.Actors,
                Plot = movie.Plot,
                Language = movie.Language,
                Country = movie.Country,
                Awards = movie.Awards,
                Poster = movie.Poster,
                Metascore = movie.Metascore,
                Rating = movie.Rating,
                Votes = movie.Votes,
                Prices = prices,
                BestPrice = bestPrice
            };
        }
    }
}
