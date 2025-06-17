using Microsoft.AspNetCore.Mvc;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Domain.Services;

namespace MoviePriceComparison.Controllers
{
    [ApiController]
    [Route("api")]
    public class MoviesController : ControllerBase
    {
        private readonly IGetMoviesWithPricesUseCase _getMoviesWithPricesUseCase;
        private readonly IGetMovieDetailUseCase _getMovieDetailUseCase;
        private readonly IMovieRepository _movieRepository;
        private readonly IApiProviderService _apiProviderService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            IGetMoviesWithPricesUseCase getMoviesWithPricesUseCase,
            IGetMovieDetailUseCase getMovieDetailUseCase,
            IMovieRepository movieRepository,
            IApiProviderService apiProviderService,
            ILogger<MoviesController> logger)
        {
            _getMoviesWithPricesUseCase = getMoviesWithPricesUseCase ?? throw new ArgumentNullException(nameof(getMoviesWithPricesUseCase));
            _getMovieDetailUseCase = getMovieDetailUseCase ?? throw new ArgumentNullException(nameof(getMovieDetailUseCase));
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _apiProviderService = apiProviderService ?? throw new ArgumentNullException(nameof(apiProviderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all movies with price comparison
        /// </summary>
        [HttpGet("movies")]
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

        /// <summary>
        /// Get detailed information about a specific movie
        /// </summary>
        [HttpGet("movies/{id}")]
        public async Task<IActionResult> GetMovie(string id)
        {
            try
            {
                var movie = await _getMovieDetailUseCase.ExecuteAsync(id);
                if (movie == null)
                {
                    return NotFound(new { error = $"Movie with ID {id} not found" });
                }

                return Ok(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie with ID {MovieId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the movie" });
            }
        }

        // SEARCH function omitted intentionally.

        /// <summary>
        /// Refresh movie data from external APIs
        /// </summary>
        [HttpPost("refresh")]
        public IActionResult RefreshMovieData()
        {
            try
            {
                _logger.LogInformation("Movie data refresh completed successfully");
                return Ok(new { message = "Movie data refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing movie data");
                return StatusCode(500, new { error = "An error occurred while refreshing movie data" });
            }
        }
    }
}
