using MoviePriceComparison.Domain.Entities;

namespace MoviePriceComparison.Domain.Repositories
{
    public interface IMovieRepository
    {
        Task<IEnumerable<MovieSummary>> GetAllAsync();
    }
}
