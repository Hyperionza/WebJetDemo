using System.ComponentModel.DataAnnotations;

namespace MoviePriceComparison.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(4)]
        public string? Year { get; set; }
        
        [MaxLength(50)]
        public string? Type { get; set; }
        
        [MaxLength(10)]
        public string? Rated { get; set; }
        
        [MaxLength(50)]
        public string? Released { get; set; }
        
        [MaxLength(50)]
        public string? Runtime { get; set; }
        
        [MaxLength(200)]
        public string? Genre { get; set; }
        
        [MaxLength(200)]
        public string? Director { get; set; }
        
        [MaxLength(500)]
        public string? Writer { get; set; }
        
        [MaxLength(1000)]
        public string? Actors { get; set; }
        
        [MaxLength(2000)]
        public string? Plot { get; set; }
        
        [MaxLength(100)]
        public string? Language { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [MaxLength(500)]
        public string? Awards { get; set; }
        
        [MaxLength(1000)]
        public string? Poster { get; set; }
        
        [MaxLength(10)]
        public string? Metascore { get; set; }
        
        [MaxLength(10)]
        public string? Rating { get; set; }
        
        [MaxLength(20)]
        public string? Votes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<MoviePrice> MoviePrices { get; set; } = new List<MoviePrice>();
    }
}
