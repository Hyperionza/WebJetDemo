using Microsoft.EntityFrameworkCore;
using MoviePriceComparison.Models;

namespace MoviePriceComparison.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MoviePrice> MoviePrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Movie entity
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Year).HasMaxLength(4);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Rated).HasMaxLength(10);
                entity.Property(e => e.Released).HasMaxLength(50);
                entity.Property(e => e.Runtime).HasMaxLength(50);
                entity.Property(e => e.Genre).HasMaxLength(200);
                entity.Property(e => e.Director).HasMaxLength(200);
                entity.Property(e => e.Writer).HasMaxLength(500);
                entity.Property(e => e.Actors).HasMaxLength(1000);
                entity.Property(e => e.Plot).HasMaxLength(2000);
                entity.Property(e => e.Language).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Awards).HasMaxLength(500);
                entity.Property(e => e.Poster).HasMaxLength(1000);
                entity.Property(e => e.Metascore).HasMaxLength(10);
                entity.Property(e => e.Rating).HasMaxLength(10);
                entity.Property(e => e.Votes).HasMaxLength(20);
                
                // Indexes for better performance
                entity.HasIndex(e => new { e.Title, e.Year }).HasDatabaseName("IX_Movie_Title_Year");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Movie_CreatedAt");
            });

            // Configure MoviePrice entity
            modelBuilder.Entity<MoviePrice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Freshness).HasConversion<string>();
                
                // Foreign key relationship
                entity.HasOne(e => e.Movie)
                      .WithMany(e => e.MoviePrices)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes for better performance
                entity.HasIndex(e => new { e.Provider, e.ExternalId }).HasDatabaseName("IX_MoviePrice_Provider_ExternalId");
                entity.HasIndex(e => e.LastUpdated).HasDatabaseName("IX_MoviePrice_LastUpdated");
                entity.HasIndex(e => e.MovieId).HasDatabaseName("IX_MoviePrice_MovieId");
                
                // Unique constraint to prevent duplicate provider entries for same movie
                entity.HasIndex(e => new { e.MovieId, e.Provider }).IsUnique().HasDatabaseName("IX_MoviePrice_MovieId_Provider_Unique");
            });
        }
    }
}
