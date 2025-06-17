using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Infrastructure.Services;
using MoviePriceComparison.Domain.Entities;
using System.Net.Http;

namespace MoviePriceComparison.Application.UseCases
{
    public interface IGetMovieDetailUseCase
    {
        Task<MovieDetailResponseDto?> ExecuteAsync(string movieId);
    }

    public class GetMovieDetailUseCase : UseCaseBase, IGetMovieDetailUseCase
    {
        public GetMovieDetailUseCase(IMovieRepository movieRepository, HttpClient httpClient) : base(movieRepository, httpClient) { }

        public async Task<MovieDetailResponseDto?> ExecuteAsync(string movieId)
        {
            var movies = await _movieRepository.GetAllAsync();
            var movie = movies.FirstOrDefault(x => x.ProviderSpecificDetails.Any(a => a.MovieId == movieId));
            if (movie == null)
                return null;

            var cheapestProvider = movie.ProviderSpecificDetails.MinBy(x => x.Price);

            // Get a valid poster URL from the provider-specific details
            var validPosterUrl = await GetValidPosterUrlAsync(movie.ProviderSpecificDetails);

            return new MovieDetailResponseDto
            {
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
                Poster = validPosterUrl,
                Metascore = movie.Metascore,
                Rating = movie.Rating,
                Votes = movie.Votes,
                UpdatedAt = movie.UpdatedAt,
                Prices = movie.ProviderSpecificDetails.Select(price => new MoviePriceDto
                {
                    ProviderId = price.ProviderId,
                    Provider = price.Provider,
                    MovieId = price.MovieId,
                    Price = price.Price,
                    LastUpdated = price.UpdatedAt,
                }).ToList(),
                CheapestPrice = cheapestProvider != null ? new MoviePriceDto
                {
                    Provider = cheapestProvider.Provider,
                    ProviderId = cheapestProvider.ProviderId,
                    MovieId = cheapestProvider.MovieId,
                    Price = cheapestProvider.Price,
                    LastUpdated = cheapestProvider.UpdatedAt
                } : null,
            };
        }
    }
}
