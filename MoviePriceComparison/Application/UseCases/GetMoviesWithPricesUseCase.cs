using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Repositories;

namespace MoviePriceComparison.Application.UseCases
{
    public interface IGetMoviesWithPricesUseCase
    {
        Task<IEnumerable<MovieComparisonDto>> ExecuteAsync();
    }

    public class GetMoviesWithPricesUseCase : UseCaseBase, IGetMoviesWithPricesUseCase
    {
        public GetMoviesWithPricesUseCase(IMovieRepository movieRepository, HttpClient httpClient) : base(movieRepository, httpClient) { }


        public async Task<IEnumerable<MovieComparisonDto>> ExecuteAsync()
        {
            List<MovieComparisonDto> result = new();
            var movies = await _movieRepository.GetAllAsync();
            foreach (var movie in movies)
            {
                var cheapestProvider = movie.ProviderSpecificDetails.MinBy(x => x.Price);

                // Get a valid poster URL from the provider-specific details
                var validPosterUrl = await GetValidPosterUrlAsync(movie.ProviderSpecificDetails);

                result.Add(new MovieComparisonDto
                {
                    Title = movie.Title,
                    Year = movie.Year,
                    Genre = movie.Genre,
                    Director = movie.Director,
                    Actors = movie.Actors,
                    Plot = movie.Plot,
                    Poster = validPosterUrl,
                    Rating = movie.Rating,
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
                    } : null
                });
            }

            return result;
        }
    }
}