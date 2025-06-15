using System.Text.Json.Serialization;

namespace MoviePriceComparison.DTOs
{
    public class MoviesListResponse
    {
        [JsonPropertyName("Movies")]
        public List<MovieSummaryDto> Movies { get; set; } = new List<MovieSummaryDto>();
    }

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

    public class MovieDetailDto
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("Year")]
        public string Year { get; set; } = string.Empty;
        
        [JsonPropertyName("Rated")]
        public string Rated { get; set; } = string.Empty;
        
        [JsonPropertyName("Released")]
        public string Released { get; set; } = string.Empty;
        
        [JsonPropertyName("Runtime")]
        public string Runtime { get; set; } = string.Empty;
        
        [JsonPropertyName("Genre")]
        public string Genre { get; set; } = string.Empty;
        
        [JsonPropertyName("Director")]
        public string Director { get; set; } = string.Empty;
        
        [JsonPropertyName("Writer")]
        public string Writer { get; set; } = string.Empty;
        
        [JsonPropertyName("Actors")]
        public string Actors { get; set; } = string.Empty;
        
        [JsonPropertyName("Plot")]
        public string Plot { get; set; } = string.Empty;
        
        [JsonPropertyName("Language")]
        public string Language { get; set; } = string.Empty;
        
        [JsonPropertyName("Country")]
        public string Country { get; set; } = string.Empty;
        
        [JsonPropertyName("Awards")]
        public string Awards { get; set; } = string.Empty;
        
        [JsonPropertyName("Poster")]
        public string Poster { get; set; } = string.Empty;
        
        [JsonPropertyName("Metascore")]
        public string Metascore { get; set; } = string.Empty;
        
        [JsonPropertyName("Rating")]
        public string Rating { get; set; } = string.Empty;
        
        [JsonPropertyName("Votes")]
        public string Votes { get; set; } = string.Empty;
        
        [JsonPropertyName("ID")]
        public string ID { get; set; } = string.Empty;
        
        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("Price")]
        public string Price { get; set; } = string.Empty;
    }
}
