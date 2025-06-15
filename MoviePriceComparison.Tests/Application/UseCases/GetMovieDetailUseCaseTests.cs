using FluentAssertions;
using Moq;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Repositories;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Application.UseCases
{
    [TestFixture]
    public class GetMovieDetailUseCaseTests
    {
        private Mock<IMovieRepository> _mockMovieRepository;
        private GetMovieDetailUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();
            _useCase = new GetMovieDetailUseCase(_mockMovieRepository.Object);
        }

        [Test]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMovieDetailUseCase(null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("movieRepository");
        }

        [Test]
        public async Task ExecuteAsync_WithExistingMovie_ShouldReturnMovieDetailDto()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("The Matrix", "1999", "movie");
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

            var price1 = new MoviePrice(movieId, "Cinemaworld", 15.99m);
            var price2 = new MoviePrice(movieId, "Filmworld", 14.99m);
            movie.AddPrice(price1);
            movie.AddPrice(price2);

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(movie.Id);
            result.Title.Should().Be("The Matrix");
            result.Year.Should().Be("1999");
            result.Type.Should().Be("movie");
            result.Rated.Should().Be("R");
            result.Released.Should().Be("31 Mar 1999");
            result.Runtime.Should().Be("136 min");
            result.Genre.Should().Be("Action, Sci-Fi");
            result.Director.Should().Be("Lana Wachowski, Lilly Wachowski");
            result.Writer.Should().Be("Lana Wachowski, Lilly Wachowski");
            result.Actors.Should().Be("Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss");
            result.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
            result.Language.Should().Be("English");
            result.Country.Should().Be("United States");
            result.Awards.Should().Be("Won 4 Oscars");
            result.Poster.Should().Be("https://example.com/matrix.jpg");
            result.Metascore.Should().Be("73");
            result.Rating.Should().Be("8.7");
            result.Votes.Should().Be("1,800,000");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Prices.Should().HaveCount(2);
            result.HasValidPrices.Should().BeTrue();
            result.CheapestPrice.Should().NotBeNull();
            result.CheapestPrice!.Provider.Should().Be("Filmworld");
            result.CheapestPrice.Price.Should().Be(14.99m);
        }

        [Test]
        public async Task ExecuteAsync_WithNonExistentMovie_ShouldReturnNull()
        {
            // Arrange
            var movieId = 999;

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync((Movie?)null);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMovieWithoutPrices_ShouldReturnDtoWithNoCheapestPrice()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("The Matrix", "1999", "movie");

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("The Matrix");
            result.Prices.Should().BeEmpty();
            result.HasValidPrices.Should().BeFalse();
            result.CheapestPrice.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMovieWithInvalidPrices_ShouldReturnDtoWithNoCheapestPrice()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("The Matrix", "1999", "movie");
            var invalidPrice1 = new MoviePrice(movieId, "Cinemaworld"); // No price
            var invalidPrice2 = new MoviePrice(movieId, "Filmworld", 0m); // Zero price

            movie.AddPrice(invalidPrice1);
            movie.AddPrice(invalidPrice2);

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("The Matrix");
            result.Prices.Should().HaveCount(2);
            result.HasValidPrices.Should().BeFalse();
            result.CheapestPrice.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_ShouldMapAllPriceProperties()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("The Matrix", "1999", "movie");
            var price = new MoviePrice(movieId, "Cinemaworld", 15.99m, "AUD");
            price.UpdatePrice(15.99m, "Updated successfully");

            movie.AddPrice(price);

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Prices.Should().HaveCount(1);
            var priceDto = result.Prices.First();

            priceDto.Provider.Should().Be("Cinemaworld");
            priceDto.Price.Should().Be(15.99m);
            priceDto.Currency.Should().Be("AUD");
            priceDto.IsAvailable.Should().BeTrue();
            priceDto.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            priceDto.ErrorMessage.Should().Be("Updated successfully");
        }

        [Test]
        public async Task ExecuteAsync_WithMultiplePrices_ShouldReturnCorrectCheapestPrice()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("The Matrix", "1999", "movie");
            var expensivePrice = new MoviePrice(movieId, "Cinemaworld", 19.99m);
            var cheapPrice = new MoviePrice(movieId, "Filmworld", 14.99m);
            var mediumPrice = new MoviePrice(movieId, "OtherProvider", 17.50m);

            movie.AddPrice(expensivePrice);
            movie.AddPrice(cheapPrice);
            movie.AddPrice(mediumPrice);

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Prices.Should().HaveCount(3);
            result.HasValidPrices.Should().BeTrue();
            result.CheapestPrice.Should().NotBeNull();
            result.CheapestPrice!.Provider.Should().Be("Filmworld");
            result.CheapestPrice.Price.Should().Be(14.99m);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            var movieId = 1;
            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync((Movie?)null);

            // Act
            await _useCase.ExecuteAsync(movieId);

            // Assert
            _mockMovieRepository.Verify(r => r.GetByIdWithPricesAsync(movieId), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var movieId = 1;
            var expectedException = new InvalidOperationException("Database error");
            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var action = async () => await _useCase.ExecuteAsync(movieId);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        [Test]
        public async Task ExecuteAsync_WithMinimalMovieData_ShouldReturnDtoWithNullOptionalFields()
        {
            // Arrange
            var movieId = 1;
            var movie = new Movie("Simple Movie", "2023", "movie");
            // Don't call UpdateDetails, so optional fields remain null

            _mockMovieRepository
                .Setup(r => r.GetByIdWithPricesAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Simple Movie");
            result.Year.Should().Be("2023");
            result.Type.Should().Be("movie");
            result.Rated.Should().BeNull();
            result.Released.Should().BeNull();
            result.Runtime.Should().BeNull();
            result.Genre.Should().BeNull();
            result.Director.Should().BeNull();
            result.Writer.Should().BeNull();
            result.Actors.Should().BeNull();
            result.Plot.Should().BeNull();
            result.Language.Should().BeNull();
            result.Country.Should().BeNull();
            result.Awards.Should().BeNull();
            result.Poster.Should().BeNull();
            result.Metascore.Should().BeNull();
            result.Rating.Should().BeNull();
            result.Votes.Should().BeNull();
        }
    }
}
