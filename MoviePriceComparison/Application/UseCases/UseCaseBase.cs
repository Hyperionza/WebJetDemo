using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Infrastructure.Services;
using MoviePriceComparison.Domain.Entities;
using System.Net.Http;

namespace MoviePriceComparison.Application.UseCases
{
    public abstract class UseCaseBase
    {
        protected readonly IMovieRepository _movieRepository;
        protected readonly HttpClient _httpClient;

        public UseCaseBase(IMovieRepository movieRepository, HttpClient httpClient)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        protected async Task<string?> GetValidPosterUrlAsync(IEnumerable<MovieProviderDetail> providerDetails)
        {
            var posterUrls = providerDetails
                .Where(p => !string.IsNullOrEmpty(p.PosterUrl))
                .Select(p => p.PosterUrl)
                .Distinct()
                .ToList();

            foreach (var url in posterUrls)
            {
                if (await IsValidImageUrlAsync(url))
                {
                    return url;
                }
            }

            return posterUrls.FirstOrDefault(); // Return first URL as fallback if none are accessible
        }

        protected async Task<bool> IsValidImageUrlAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return false;
                }

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                    return false;

                var contentType = response.Content.Headers.ContentType?.MediaType;
                return contentType != null && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
