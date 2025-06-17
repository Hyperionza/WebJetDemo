namespace MoviePriceComparison.Application.DTOs
{
    public class MovieComparisonDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Year { get; set; }
        public string? Genre { get; set; }
        public string? Director { get; set; }
        public string? Actors { get; set; }
        public string? Plot { get; set; }
        public string? Poster { get; set; }
        public string? Rating { get; set; }
        public List<MoviePriceDto> Prices { get; set; } = new();
        public MoviePriceDto? CheapestPrice { get; set; }
    }
}
