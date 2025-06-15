using Microsoft.EntityFrameworkCore;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Infrastructure.Data;

namespace MoviePriceComparison.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly MovieDbContext _context;

        public MovieRepository(MovieDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _context.Movies
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Movie?> GetByIdWithPricesAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MoviePrices)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetAllWithPricesAsync()
        {
            return await _context.Movies
                .Include(m => m.MoviePrices)
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllWithPricesAsync();

            var searchTerm = query.ToLower();

            return await _context.Movies
                .Include(m => m.MoviePrices)
                .Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    (m.Genre != null && m.Genre.ToLower().Contains(searchTerm)) ||
                    (m.Director != null && m.Director.ToLower().Contains(searchTerm)) ||
                    (m.Actors != null && m.Actors.ToLower().Contains(searchTerm)))
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<Movie> AddAsync(Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task UpdateAsync(Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Movies.AnyAsync(m => m.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
