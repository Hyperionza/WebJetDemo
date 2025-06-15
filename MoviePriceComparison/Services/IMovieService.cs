using MoviePriceComparison.DTOs;

namespace MoviePriceComparison.Services
{
    public interface IMovieService
    {
        Task<List<MovieComparisonDto>> GetMoviesWithPricesAsync();
        Task<MovieDetailResponseDto?> GetMovieDetailAsync(int movieId);
        Task<List<MovieComparisonDto>> SearchMoviesAsync(string query);
        Task RefreshMovieDataAsync();
        Task<List<ApiHealthDto>> GetApiHealthAsync();
    }
}
