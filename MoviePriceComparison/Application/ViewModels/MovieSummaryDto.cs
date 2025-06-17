using System.Text.Json.Serialization;

namespace MoviePriceComparison.Application.DTOs
{
    public class MovieSummaryDto
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("Year")]
        public string Year { get; set; } = string.Empty;

        [JsonPropertyName("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("Poster")]
        public string Poster { get; set; } = string.Empty;
    }
}
