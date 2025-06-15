using MoviePriceComparison.Domain.Entities;

namespace MoviePriceComparison.Domain.Repositories
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie?> GetByIdAsync(int id);
        Task<Movie?> GetByIdWithPricesAsync(int id);
        Task<IEnumerable<Movie>> GetAllWithPricesAsync();
        Task<IEnumerable<Movie>> SearchAsync(string query);
        Task<Movie> AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }
}
