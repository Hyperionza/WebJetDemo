using MoviePriceComparison.Infrastructure.Services;

namespace MoviePriceComparison.Domain.Services
{
    public interface IExternalMovieApiService
    {
        Task<IEnumerable<ExternalMovieSummaryDto>> GetMoviesFromProviderAsync(string provider);
        Task<ExternalMovieDetailDto?> GetMovieDetailsFromProviderAsync(string provider, string movieId);
        // would be nice to have something like this
        // Task<bool> IsProviderHealthyAsync(string provider);
    }
}
