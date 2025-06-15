using MoviePriceComparison.DTOs;

namespace MoviePriceComparison.Services
{
    public interface IExternalApiService
    {
        Task<MoviesListResponse?> GetMoviesAsync(string provider);
        Task<MovieDetailDto?> GetMovieDetailAsync(string provider, string movieId);
        Task<bool> IsHealthyAsync(string provider);
    }
}
