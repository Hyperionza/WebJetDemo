namespace MoviePriceComparison.Domain.Entities
{
    public class Movie
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string? Year { get; private set; }
        public string? Type { get; private set; }
        public string? Rated { get; private set; }
        public string? Released { get; private set; }
        public string? Runtime { get; private set; }
        public string? Genre { get; private set; }
        public string? Director { get; private set; }
        public string? Writer { get; private set; }
        public string? Actors { get; private set; }
        public string? Plot { get; private set; }
        public string? Language { get; private set; }
        public string? Country { get; private set; }
        public string? Awards { get; private set; }
        public string? Poster { get; private set; }
        public string? Metascore { get; private set; }
        public string? Rating { get; private set; }
        public string? Votes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<MoviePrice> _moviePrices = new();
        public IReadOnlyCollection<MoviePrice> MoviePrices => _moviePrices.AsReadOnly();

        // Private constructor for EF Core
        private Movie() { }

        public Movie(string title, string? year = null, string? type = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty", nameof(title));

            Title = title;
            Year = year;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(
            string? year = null,
            string? type = null,
            string? rated = null,
            string? released = null,
            string? runtime = null,
            string? genre = null,
            string? director = null,
            string? writer = null,
            string? actors = null,
            string? plot = null,
            string? language = null,
            string? country = null,
            string? awards = null,
            string? poster = null,
            string? metascore = null,
            string? rating = null,
            string? votes = null)
        {
            Year = year ?? Year;
            Type = type ?? Type;
            Rated = rated ?? Rated;
            Released = released ?? Released;
            Runtime = runtime ?? Runtime;
            Genre = genre ?? Genre;
            Director = director ?? Director;
            Writer = writer ?? Writer;
            Actors = actors ?? Actors;
            Plot = plot ?? Plot;
            Language = language ?? Language;
            Country = country ?? Country;
            Awards = awards ?? Awards;
            Poster = poster ?? Poster;
            Metascore = metascore ?? Metascore;
            Rating = rating ?? Rating;
            Votes = votes ?? Votes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddPrice(MoviePrice price)
        {
            if (price == null)
                throw new ArgumentNullException(nameof(price));

            _moviePrices.Add(price);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearPrices()
        {
            _moviePrices.Clear();
            UpdatedAt = DateTime.UtcNow;
        }

        public MoviePrice? GetCheapestPrice()
        {
            return _moviePrices
                .Where(p => p.Price.HasValue && p.Price > 0)
                .OrderBy(p => p.Price!.Value)
                .FirstOrDefault();
        }

        public bool HasValidPrices()
        {
            return _moviePrices.Any(p => p.Price.HasValue && p.Price > 0);
        }
    }
}
