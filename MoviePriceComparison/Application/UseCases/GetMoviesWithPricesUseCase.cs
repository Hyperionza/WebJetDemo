using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Repositories;

namespace MoviePriceComparison.Application.UseCases
{
    public interface IGetMoviesWithPricesUseCase
    {
        Task<IEnumerable<MovieComparisonDto>> ExecuteAsync();
    }

    public class GetMoviesWithPricesUseCase : IGetMoviesWithPricesUseCase
    {
        private readonly IMovieRepository _movieRepository;

        public GetMoviesWithPricesUseCase(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        }

        public async Task<IEnumerable<MovieComparisonDto>> ExecuteAsync()
        {
            var movies = await _movieRepository.GetAllWithPricesAsync();

            return movies.Select(movie => new MovieComparisonDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Genre = movie.Genre,
                Director = movie.Director,
                Actors = movie.Actors,
                Plot = movie.Plot,
                Poster = movie.Poster,
                Rating = movie.Rating,
                Prices = movie.MoviePrices.Select(price => new MoviePriceDto
                {
                    Provider = price.Provider,
                    Price = price.Price,
                    Currency = price.Currency,
                    IsAvailable = price.IsAvailable,
                    LastUpdated = price.LastUpdated,
                    ErrorMessage = price.ErrorMessage
                }).ToList(),
                CheapestPrice = movie.GetCheapestPrice() != null ? new MoviePriceDto
                {
                    Provider = movie.GetCheapestPrice()!.Provider,
                    Price = movie.GetCheapestPrice()!.Price,
                    Currency = movie.GetCheapestPrice()!.Currency,
                    IsAvailable = movie.GetCheapestPrice()!.IsAvailable,
                    LastUpdated = movie.GetCheapestPrice()!.LastUpdated,
                    ErrorMessage = movie.GetCheapestPrice()!.ErrorMessage
                } : null,
                HasValidPrices = movie.HasValidPrices()
            });
        }
    }
}
