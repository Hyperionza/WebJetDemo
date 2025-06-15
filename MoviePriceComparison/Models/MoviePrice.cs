using System.ComponentModel.DataAnnotations;

namespace MoviePriceComparison.Models
{
    public enum DataFreshness
    {
        Fresh,      // Retrieved within cache duration (5 minutes)
        Cached,     // Retrieved from cache due to API failure
        Stale       // Cache expired and API still failing
    }

    public class MoviePrice
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty; // "Cinemaworld" or "Filmworld"

        [Required]
        [MaxLength(50)]
        public string ExternalId { get; set; } = string.Empty; // Provider's movie ID (e.g., "cw0076759", "fw0076759")

        public decimal Price { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public DataFreshness Freshness { get; set; } = DataFreshness.Fresh;

        // Navigation properties
        public virtual Movie Movie { get; set; } = null!;
    }
}
