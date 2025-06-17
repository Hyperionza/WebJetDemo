using System.Text.Json.Serialization;

namespace MoviePriceComparison.Application.DTOs
{
    public class MoviesListResponse
    {
        [JsonPropertyName("Movies")]
        public List<MovieSummaryDto> Movies { get; set; } = new List<MovieSummaryDto>();
    }
}
