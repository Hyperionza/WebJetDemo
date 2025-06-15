using MoviePriceComparison.Domain.Entities;

namespace MoviePriceComparison.Domain.Services
{
    public interface IExternalMovieApiService
    {
        Task<IEnumerable<Movie>> GetMoviesFromProviderAsync(string provider);
        Task<Movie?> GetMovieDetailsFromProviderAsync(string provider, string movieId);
        Task<decimal?> GetMoviePriceFromProviderAsync(string provider, string movieId);
        Task<bool> IsProviderHealthyAsync(string provider);
    }
}
