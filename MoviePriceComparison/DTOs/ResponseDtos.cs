using MoviePriceComparison.Models;

namespace MoviePriceComparison.DTOs
{
    public class MovieComparisonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Year { get; set; }
        public string? Genre { get; set; }
        public string? Director { get; set; }
        public string? Poster { get; set; }
        public string? Rating { get; set; }
        public List<PriceInfoDto> Prices { get; set; } = new List<PriceInfoDto>();
        public PriceInfoDto? BestPrice { get; set; }
    }

    public class PriceInfoDto
    {
        public string Provider { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DataFreshness Freshness { get; set; }
        public DateTime LastUpdated { get; set; }
        public string FreshnessIndicator => Freshness switch
        {
            DataFreshness.Fresh => "🟢",
            DataFreshness.Cached => "🟡",
            DataFreshness.Stale => "🔴",
            _ => "⚪"
        };
    }

    public class MovieDetailResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Year { get; set; }
        public string? Rated { get; set; }
        public string? Released { get; set; }
        public string? Runtime { get; set; }
        public string? Genre { get; set; }
        public string? Director { get; set; }
        public string? Writer { get; set; }
        public string? Actors { get; set; }
        public string? Plot { get; set; }
        public string? Language { get; set; }
        public string? Country { get; set; }
        public string? Awards { get; set; }
        public string? Poster { get; set; }
        public string? Metascore { get; set; }
        public string? Rating { get; set; }
        public string? Votes { get; set; }
        public List<PriceInfoDto> Prices { get; set; } = new List<PriceInfoDto>();
        public PriceInfoDto? BestPrice { get; set; }
    }

    public class ApiHealthDto
    {
        public string Provider { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public DateTime LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
