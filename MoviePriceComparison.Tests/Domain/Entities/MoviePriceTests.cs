using FluentAssertions;
using MoviePriceComparison.Domain.Entities;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Domain.Entities
{
    [TestFixture]
    public class MoviePriceTests
    {
        [Test]
        public void Constructor_WithValidParameters_ShouldCreateMoviePrice()
        {
            // Arrange
            var movieId = 1;
            var provider = "Cinemaworld";
            var price = 15.99m;
            var currency = "AUD";

            // Act
            var moviePrice = new MoviePrice(movieId, provider, price, currency);

            // Assert
            moviePrice.MovieId.Should().Be(movieId);
            moviePrice.Provider.Should().Be(provider);
            moviePrice.Price.Should().Be(price);
            moviePrice.Currency.Should().Be(currency);
            moviePrice.IsAvailable.Should().BeTrue();
            moviePrice.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            moviePrice.ErrorMessage.Should().BeNull();
        }

        [Test]
        public void Constructor_WithNullPrice_ShouldCreateUnavailableMoviePrice()
        {
            // Arrange
            var movieId = 1;
            var provider = "Cinemaworld";
            decimal? price = null;

            // Act
            var moviePrice = new MoviePrice(movieId, provider, price);

            // Assert
            moviePrice.MovieId.Should().Be(movieId);
            moviePrice.Provider.Should().Be(provider);
            moviePrice.Price.Should().BeNull();
            moviePrice.IsAvailable.Should().BeFalse();
            moviePrice.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Constructor_WithZeroPrice_ShouldCreateUnavailableMoviePrice()
        {
            // Arrange
            var movieId = 1;
            var provider = "Cinemaworld";
            var price = 0m;

            // Act
            var moviePrice = new MoviePrice(movieId, provider, price);

            // Assert
            moviePrice.Price.Should().Be(0m);
            moviePrice.IsAvailable.Should().BeFalse();
        }

        [Test]
        public void Constructor_WithNegativePrice_ShouldCreateUnavailableMoviePrice()
        {
            // Arrange
            var movieId = 1;
            var provider = "Cinemaworld";
            var price = -5.99m;

            // Act
            var moviePrice = new MoviePrice(movieId, provider, price);

            // Assert
            moviePrice.Price.Should().Be(-5.99m);
            moviePrice.IsAvailable.Should().BeFalse();
        }

        [Test]
        public void Constructor_WithNullProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var movieId = 1;
            string? provider = null;
            var price = 15.99m;

            // Act & Assert
            var action = () => new MoviePrice(movieId, provider!, price);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Provider cannot be null or empty*");
        }

        [Test]
        public void Constructor_WithEmptyProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var movieId = 1;
            var provider = "";
            var price = 15.99m;

            // Act & Assert
            var action = () => new MoviePrice(movieId, provider, price);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Provider cannot be null or empty*");
        }

        [Test]
        public void Constructor_WithWhitespaceProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var movieId = 1;
            var provider = "   ";
            var price = 15.99m;

            // Act & Assert
            var action = () => new MoviePrice(movieId, provider, price);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Provider cannot be null or empty*");
        }

        [Test]
        public void UpdatePrice_WithValidPrice_ShouldUpdatePriceAndMakeAvailable()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld");
            var newPrice = 15.99m;
            var originalLastUpdated = moviePrice.LastUpdated;

            // Wait a small amount to ensure LastUpdated changes
            Thread.Sleep(10);

            // Act
            moviePrice.UpdatePrice(newPrice);

            // Assert
            moviePrice.Price.Should().Be(newPrice);
            moviePrice.IsAvailable.Should().BeTrue();
            moviePrice.ErrorMessage.Should().BeNull();
            moviePrice.LastUpdated.Should().BeAfter(originalLastUpdated);
        }

        [Test]
        public void UpdatePrice_WithNullPrice_ShouldMakeUnavailable()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            var originalLastUpdated = moviePrice.LastUpdated;

            // Wait a small amount to ensure LastUpdated changes
            Thread.Sleep(10);

            // Act
            moviePrice.UpdatePrice(null);

            // Assert
            moviePrice.Price.Should().BeNull();
            moviePrice.IsAvailable.Should().BeFalse();
            moviePrice.ErrorMessage.Should().BeNull();
            moviePrice.LastUpdated.Should().BeAfter(originalLastUpdated);
        }

        [Test]
        public void UpdatePrice_WithZeroPrice_ShouldMakeUnavailable()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);

            // Act
            moviePrice.UpdatePrice(0m);

            // Assert
            moviePrice.Price.Should().Be(0m);
            moviePrice.IsAvailable.Should().BeFalse();
        }

        [Test]
        public void UpdatePrice_WithErrorMessage_ShouldSetErrorMessage()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld");
            var errorMessage = "API temporarily unavailable";

            // Act
            moviePrice.UpdatePrice(null, errorMessage);

            // Assert
            moviePrice.Price.Should().BeNull();
            moviePrice.IsAvailable.Should().BeFalse();
            moviePrice.ErrorMessage.Should().Be(errorMessage);
        }

        [Test]
        public void MarkAsUnavailable_ShouldSetUnavailableState()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            var errorMessage = "Service temporarily down";
            var originalLastUpdated = moviePrice.LastUpdated;

            // Wait a small amount to ensure LastUpdated changes
            Thread.Sleep(10);

            // Act
            moviePrice.MarkAsUnavailable(errorMessage);

            // Assert
            moviePrice.Price.Should().BeNull();
            moviePrice.IsAvailable.Should().BeFalse();
            moviePrice.ErrorMessage.Should().Be(errorMessage);
            moviePrice.LastUpdated.Should().BeAfter(originalLastUpdated);
        }

        [Test]
        public void MarkAsUnavailable_WithoutErrorMessage_ShouldSetUnavailableState()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);

            // Act
            moviePrice.MarkAsUnavailable();

            // Assert
            moviePrice.Price.Should().BeNull();
            moviePrice.IsAvailable.Should().BeFalse();
            moviePrice.ErrorMessage.Should().BeNull();
        }

        [Test]
        public void IsStale_WithRecentUpdate_ShouldReturnFalse()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            var maxAge = TimeSpan.FromMinutes(5);

            // Act
            var result = moviePrice.IsStale(maxAge);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsStale_WithOldUpdate_ShouldReturnTrue()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            var maxAge = TimeSpan.FromMilliseconds(1);

            // Wait longer than maxAge
            Thread.Sleep(10);

            // Act
            var result = moviePrice.IsStale(maxAge);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsValid_WithValidPrice_ShouldReturnTrue()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);

            // Act
            var result = moviePrice.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsValid_WithNullPrice_ShouldReturnFalse()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld");

            // Act
            var result = moviePrice.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsValid_WithZeroPrice_ShouldReturnFalse()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 0m);

            // Act
            var result = moviePrice.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsValid_WithNegativePrice_ShouldReturnFalse()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", -5.99m);

            // Act
            var result = moviePrice.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsValid_WhenMarkedAsUnavailable_ShouldReturnFalse()
        {
            // Arrange
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);
            moviePrice.MarkAsUnavailable();

            // Act
            var result = moviePrice.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DefaultCurrency_ShouldBeAUD()
        {
            // Arrange & Act
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m);

            // Assert
            moviePrice.Currency.Should().Be("AUD");
        }

        [Test]
        public void CustomCurrency_ShouldBeSet()
        {
            // Arrange & Act
            var moviePrice = new MoviePrice(1, "Cinemaworld", 15.99m, "USD");

            // Assert
            moviePrice.Currency.Should().Be("USD");
        }
    }
}
