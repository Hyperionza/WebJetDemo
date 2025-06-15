namespace MoviePriceComparison.Domain.Entities
{
    public class MoviePrice
    {
        public int Id { get; private set; }
        public int MovieId { get; private set; }
        public string Provider { get; private set; }
        public decimal? Price { get; private set; }
        public string? Currency { get; private set; }
        public bool IsAvailable { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public string? ErrorMessage { get; private set; }

        // Navigation property
        public Movie Movie { get; private set; } = null!;

        // Private constructor for EF Core
        private MoviePrice() { }

        public MoviePrice(int movieId, string provider, decimal? price = null, string currency = "AUD")
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be null or empty", nameof(provider));

            MovieId = movieId;
            Provider = provider;
            Price = price;
            Currency = currency;
            IsAvailable = price.HasValue && price > 0;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal? price, string? errorMessage = null)
        {
            Price = price;
            IsAvailable = price.HasValue && price > 0;
            ErrorMessage = errorMessage;
            LastUpdated = DateTime.UtcNow;
        }

        public void MarkAsUnavailable(string? errorMessage = null)
        {
            Price = null;
            IsAvailable = false;
            ErrorMessage = errorMessage;
            LastUpdated = DateTime.UtcNow;
        }

        public bool IsStale(TimeSpan maxAge)
        {
            return DateTime.UtcNow - LastUpdated > maxAge;
        }

        public bool IsValid()
        {
            return IsAvailable && Price.HasValue && Price > 0;
        }
    }
}
