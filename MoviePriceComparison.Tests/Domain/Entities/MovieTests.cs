using FluentAssertions;
using MoviePriceComparison.Domain.Entities;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Domain.Entities
{
    [TestFixture]
    public class MovieTests
    {
        [Test]
        public void Constructor_WithValidTitle_ShouldCreateMovie()
        {
            // Arrange
            var title = "The Matrix";
            var year = "1999";
            var type = "movie";

            // Act
            var movie = new Movie(title, year, type);

            // Assert
            movie.Title.Should().Be(title);
            movie.Year.Should().Be(year);
            movie.Type.Should().Be(type);
            movie.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            movie.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            movie.MoviePrices.Should().BeEmpty();
        }

        [Test]
        public void Constructor_WithNullTitle_ShouldThrowArgumentException()
        {
            // Arrange
            string? title = null;

            // Act & Assert
            var action = () => new Movie(title!, "1999", "movie");
            action.Should().Throw<ArgumentException>()
                .WithMessage("Title cannot be null or empty*");
        }

        [Test]
        public void Constructor_WithEmptyTitle_ShouldThrowArgumentException()
        {
            // Arrange
            var title = "";

            // Act & Assert
            var action = () => new Movie(title, "1999", "movie");
            action.Should().Throw<ArgumentException>()
                .WithMessage("Title cannot be null or empty*");
        }

        [Test]
        public void AddPrice_WithValidPrice_ShouldAddPriceToCollection()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var price = new MoviePrice(1, "Cinemaworld", 15.99m);

            // Act
            movie.AddPrice(price);

            // Assert
            movie.MoviePrices.Should().HaveCount(1);
            movie.MoviePrices.Should().Contain(price);
            movie.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void AddPrice_WithNullPrice_ShouldThrowArgumentNullException()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");

            // Act & Assert
            var action = () => movie.AddPrice(null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("price");
        }

        [Test]
        public void GetCheapestPrice_WithMultiplePrices_ShouldReturnLowestPrice()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var expensivePrice = new MoviePrice(1, "Cinemaworld", 19.99m);
            var cheapPrice = new MoviePrice(1, "Filmworld", 14.99m);
            var mediumPrice = new MoviePrice(1, "OtherProvider", 17.50m);

            movie.AddPrice(expensivePrice);
            movie.AddPrice(cheapPrice);
            movie.AddPrice(mediumPrice);

            // Act
            var result = movie.GetCheapestPrice();

            // Assert
            result.Should().Be(cheapPrice);
            result!.Price.Should().Be(14.99m);
            result.Provider.Should().Be("Filmworld");
        }

        [Test]
        public void GetCheapestPrice_WithNoPrices_ShouldReturnNull()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");

            // Act
            var result = movie.GetCheapestPrice();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetCheapestPrice_WithOnlyNullPrices_ShouldReturnNull()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var priceWithoutValue = new MoviePrice(1, "Cinemaworld");
            movie.AddPrice(priceWithoutValue);

            // Act
            var result = movie.GetCheapestPrice();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void HasValidPrices_WithValidPrices_ShouldReturnTrue()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var validPrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            movie.AddPrice(validPrice);

            // Act
            var result = movie.HasValidPrices();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void HasValidPrices_WithNoPrices_ShouldReturnFalse()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");

            // Act
            var result = movie.HasValidPrices();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void HasValidPrices_WithOnlyInvalidPrices_ShouldReturnFalse()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var invalidPrice1 = new MoviePrice(1, "Cinemaworld"); // No price
            var invalidPrice2 = new MoviePrice(1, "Filmworld", 0m); // Zero price
            movie.AddPrice(invalidPrice1);
            movie.AddPrice(invalidPrice2);

            // Act
            var result = movie.HasValidPrices();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ClearPrices_ShouldRemoveAllPrices()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var price1 = new MoviePrice(1, "Cinemaworld", 15.99m);
            var price2 = new MoviePrice(1, "Filmworld", 14.99m);
            movie.AddPrice(price1);
            movie.AddPrice(price2);

            // Act
            movie.ClearPrices();

            // Assert
            movie.MoviePrices.Should().BeEmpty();
            movie.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void UpdateDetails_ShouldUpdateMovieProperties()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var originalUpdatedAt = movie.UpdatedAt;

            // Wait a small amount to ensure UpdatedAt changes
            Thread.Sleep(10);

            // Act
            movie.UpdateDetails(
                year: "1999",
                type: "movie",
                rated: "R",
                released: "31 Mar 1999",
                runtime: "136 min",
                genre: "Action, Sci-Fi",
                director: "Lana Wachowski, Lilly Wachowski",
                writer: "Lana Wachowski, Lilly Wachowski",
                actors: "Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss",
                plot: "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.",
                language: "English",
                country: "United States",
                awards: "Won 4 Oscars",
                poster: "https://example.com/matrix.jpg",
                metascore: "73",
                rating: "8.7",
                votes: "1,800,000"
            );

            // Assert
            movie.Rated.Should().Be("R");
            movie.Released.Should().Be("31 Mar 1999");
            movie.Runtime.Should().Be("136 min");
            movie.Genre.Should().Be("Action, Sci-Fi");
            movie.Director.Should().Be("Lana Wachowski, Lilly Wachowski");
            movie.Writer.Should().Be("Lana Wachowski, Lilly Wachowski");
            movie.Actors.Should().Be("Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss");
            movie.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
            movie.Language.Should().Be("English");
            movie.Country.Should().Be("United States");
            movie.Awards.Should().Be("Won 4 Oscars");
            movie.Poster.Should().Be("https://example.com/matrix.jpg");
            movie.Metascore.Should().Be("73");
            movie.Rating.Should().Be("8.7");
            movie.Votes.Should().Be("1,800,000");
            movie.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Test]
        public void UpdateDetails_WithNullValues_ShouldKeepExistingValues()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            movie.UpdateDetails(genre: "Action");

            // Act
            movie.UpdateDetails(director: "Wachowski Sisters"); // Only update director

            // Assert
            movie.Genre.Should().Be("Action"); // Should remain unchanged
            movie.Director.Should().Be("Wachowski Sisters"); // Should be updated
        }
    }
}
