namespace MoviePriceComparison.Application.DTOs
{
    public class MoviePriceDto
    {
        public required string ProviderId { get; set; }
        public required string MovieId { get; set; }
        public required string Provider { get; set; }
        public decimal? Price { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
