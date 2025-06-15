using Microsoft.AspNetCore.Mvc;
using MoviePriceComparison.Application.UseCases;

namespace MoviePriceComparison.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IGetMoviesWithPricesUseCase _getMoviesWithPricesUseCase;
        private readonly IGetMovieDetailUseCase _getMovieDetailUseCase;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            IGetMoviesWithPricesUseCase getMoviesWithPricesUseCase,
            IGetMovieDetailUseCase getMovieDetailUseCase,
            ILogger<MoviesController> logger)
        {
            _getMoviesWithPricesUseCase = getMoviesWithPricesUseCase ?? throw new ArgumentNullException(nameof(getMoviesWithPricesUseCase));
            _getMovieDetailUseCase = getMovieDetailUseCase ?? throw new ArgumentNullException(nameof(getMovieDetailUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all movies with price comparison
        /// </summary>
        [HttpGet]
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
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

        /// <summary>
        /// Search movies by title, genre, director, or actors
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { error = "Search query cannot be empty" });
                }

                // For now, we'll use the get all movies use case and filter
                // In a real implementation, we'd create a SearchMoviesUseCase
                // that could start covering fuzzy matching.
                var allMovies = await _getMoviesWithPricesUseCase.ExecuteAsync();
                var searchTerm = query.ToLower();

                var filteredMovies = allMovies.Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    (m.Genre?.ToLower().Contains(searchTerm) ?? false) ||
                    (m.Director?.ToLower().Contains(searchTerm) ?? false) ||
                    (m.Actors?.ToLower().Contains(searchTerm) ?? false));

                return Ok(filteredMovies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query '{Query}'", query);
                return StatusCode(500, new { error = "An error occurred while searching movies" });
            }
        }
    }
}
