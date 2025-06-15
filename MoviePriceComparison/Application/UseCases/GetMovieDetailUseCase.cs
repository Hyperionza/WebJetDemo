using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Repositories;

namespace MoviePriceComparison.Application.UseCases
{
    public interface IGetMovieDetailUseCase
    {
        Task<MovieDetailResponseDto?> ExecuteAsync(int movieId);
    }

    public class GetMovieDetailUseCase : IGetMovieDetailUseCase
    {
        private readonly IMovieRepository _movieRepository;

        public GetMovieDetailUseCase(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        }

        public async Task<MovieDetailResponseDto?> ExecuteAsync(int movieId)
        {
            var movie = await _movieRepository.GetByIdWithPricesAsync(movieId);

            if (movie == null)
                return null;

            return new MovieDetailResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Type = movie.Type,
                Rated = movie.Rated,
                Released = movie.Released,
                Runtime = movie.Runtime,
                Genre = movie.Genre,
                Director = movie.Director,
                Writer = movie.Writer,
                Actors = movie.Actors,
                Plot = movie.Plot,
                Language = movie.Language,
                Country = movie.Country,
                Awards = movie.Awards,
                Poster = movie.Poster,
                Metascore = movie.Metascore,
                Rating = movie.Rating,
                Votes = movie.Votes,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
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
            };
        }
    }
}
