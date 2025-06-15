namespace MoviePriceComparison.Domain.Entities
{
    public class MovieProviderSpecificDetail
    {
        public required string ProviderId { get; set; }
        public required string MovieId { get; set; }
        public string? PosterUrl { get; set; }
        public decimal? Price { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsStale(TimeSpan maxAge)
        {
            return DateTime.UtcNow - UpdatedAt > maxAge;
        }

    }
}
