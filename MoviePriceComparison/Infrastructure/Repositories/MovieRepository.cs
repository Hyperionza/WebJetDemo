using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MoviePriceComparison.Domain.Services;
using System.Diagnostics.Contracts;
using MoviePriceComparison.Infrastructure.Services;

namespace MoviePriceComparison.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMemoryCache _cache;
        private readonly IExternalMovieApiService _externalMovieApiService;
        private readonly IApiProviderService _apiProviderService;
        private readonly ILogger<MovieRepository> _logger;

        private const string LIST_CACHE_KEY = "movies_list";
        private const string DETAIL_CACHE_KEY = "movies_detail";

        public MovieRepository(IMemoryCache cache,
        IExternalMovieApiService externalMovieApiService,
        IApiProviderService apiProviderService,
        ILogger<MovieRepository> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _externalMovieApiService = externalMovieApiService ?? throw new ArgumentNullException(nameof(externalMovieApiService));
            _apiProviderService = apiProviderService ?? throw new ArgumentNullException(nameof(apiProviderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RefreshData()
        {
            _logger.LogInformation("Refreshing Movies cache");
            _cache.Remove(LIST_CACHE_KEY);
            _cache.Remove(DETAIL_CACHE_KEY);
            await _apiProviderService.RefreshApiProvidersAsync(); // might not be necessary
            await GetAllAsync();

        }

        public async Task<IEnumerable<MovieSummary>> GetAllAsync()
        {
            List<MovieSummary> result = new();
            var providers = await _apiProviderService.GetApiProvidersAsync();
            providers.ForEach(async p =>
            {
                if (p.IsEnabled)
                {
                    List<ExternalMovieSummaryDto>? movies = (await _externalMovieApiService.GetMoviesFromProviderAsync(p.Id)).ToList();
                    foreach (var item in movies)
                    {
                        ExternalMovieDetailDto? externalSpecifics = await _externalMovieApiService.GetMovieDetailsFromProviderAsync(p.Id, item.ID);
                        MovieSummary? currentMovie = result.FirstOrDefault(x => x.Title == item.Title);
                        if (currentMovie == null)
                        {
                            // add summary
                            currentMovie = new MovieSummary { Title = item.Title };
                            result.Add(currentMovie);
                        }
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
                        currentMovie.Type = externalSpecifics?.Type ?? item.Type ?? currentMovie.Type;
                        currentMovie.Votes = externalSpecifics?.Votes ?? currentMovie.Votes;
                        currentMovie.Writer = externalSpecifics?.Writer ?? currentMovie.Writer;
                        currentMovie.Year = externalSpecifics?.Year ?? item.Year ?? currentMovie.Year;
                        currentMovie.UpdatedAt = DateTime.UtcNow;

                        if (externalSpecifics != null)
                        {
                            var currentSpecifics = currentMovie.ProviderSpecificDetails.FirstOrDefault(x => x.ProviderId == p.Id);
                            if (currentSpecifics == null)
                            {
                                currentSpecifics = new MovieProviderSpecificDetail
                                {
                                    ProviderId = p.Id,
                                    MovieId = item.ID
                                };
                            }
                            currentSpecifics.PosterUrl = externalSpecifics?.Poster ?? currentSpecifics.PosterUrl;
                            if (externalSpecifics?.Price != null
                                && decimal.TryParse(externalSpecifics?.Price, out decimal dPrice))
                            {
                                currentSpecifics.Price = dPrice;
                            }
                            currentSpecifics.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
            });
            return result;
        }

        public Task<MovieSummary?> GetByTitleAsync(string title)
        {
            throw new NotImplementedException();
        }

        public Task ResetSummary()
        {
            throw new NotImplementedException();
        }

        public Task ResetDetail(string title)
        {
            throw new NotImplementedException();
        }
    }
}
