namespace MoviePriceComparison.Domain.Entities
{
    public class MovieSummary
    {
        public string Title { get; set; } = null!;
        public string? Year { get; set; }
        public string? Type { get; set; }
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
        public string? Metascore { get; set; }
        public string? Rating { get; set; }
        public string? Votes { get; set; }
        public List<MovieProviderDetail> ProviderSpecificDetails { get; set; } = new();
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
            Metascore = metascore ?? Metascore;
            Rating = rating ?? Rating;
            Votes = votes ?? Votes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddDetail(MovieProviderDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            // remove previous if exists
            ProviderSpecificDetails.RemoveAll(x => x.ProviderId == detail.ProviderId && x.MovieId == detail.MovieId);

            ProviderSpecificDetails.Add(detail);
            UpdatedAt = DateTime.UtcNow;
        }

        public MovieProviderDetail? GetCheapestPrice()
        {
            return ProviderSpecificDetails
                .Where(p => p.Price.HasValue && p.Price > 0)
                .OrderBy(p => p.Price!.Value)
                .FirstOrDefault();
        }
    }
}
