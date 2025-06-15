using Microsoft.EntityFrameworkCore;
using MoviePriceComparison.Domain.Entities;

namespace MoviePriceComparison.Infrastructure.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<MoviePrice> MoviePrices { get; set; } = null!;

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
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                // Configure relationships
                entity.HasMany(e => e.MoviePrices)
                      .WithOne(e => e.Movie)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MoviePrice entity
            modelBuilder.Entity<MoviePrice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.IsAvailable).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);

                // Create index for better query performance
                entity.HasIndex(e => new { e.MovieId, e.Provider }).IsUnique();
                entity.HasIndex(e => e.LastUpdated);
            });
        }
    }
}
