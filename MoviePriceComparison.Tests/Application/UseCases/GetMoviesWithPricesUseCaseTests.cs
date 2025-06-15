using FluentAssertions;
using Moq;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Repositories;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Application.UseCases
{
    [TestFixture]
    public class GetMoviesWithPricesUseCaseTests
    {
        private Mock<IMovieRepository> _mockMovieRepository;
        private GetMoviesWithPricesUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();
            _useCase = new GetMoviesWithPricesUseCase(_mockMovieRepository.Object);
        }

        [Test]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMoviesWithPricesUseCase(null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("movieRepository");
        }

        [Test]
        public async Task ExecuteAsync_WithMoviesAndPrices_ShouldReturnMovieComparisonDtos()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            var movie2 = new Movie("Inception", "2010", "movie");

            var price1 = new MoviePrice(1, "Cinemaworld", 15.99m);
            var price2 = new MoviePrice(1, "Filmworld", 14.99m);
            var price3 = new MoviePrice(2, "Cinemaworld", 18.99m);

            movie1.AddPrice(price1);
            movie1.AddPrice(price2);
            movie2.AddPrice(price3);

            movie1.UpdateDetails(genre: "Action, Sci-Fi", director: "Wachowski Sisters", actors: "Keanu Reeves");
            movie2.UpdateDetails(genre: "Action, Thriller", director: "Christopher Nolan", actors: "Leonardo DiCaprio");

            var movies = new List<Movie> { movie1, movie2 };

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(2);

            var movie1Dto = result.First(m => m.Title == "The Matrix");
            movie1Dto.Id.Should().Be(movie1.Id);
            movie1Dto.Title.Should().Be("The Matrix");
            movie1Dto.Year.Should().Be("1999");
            movie1Dto.Genre.Should().Be("Action, Sci-Fi");
            movie1Dto.Director.Should().Be("Wachowski Sisters");
            movie1Dto.Actors.Should().Be("Keanu Reeves");
            movie1Dto.Prices.Should().HaveCount(2);
            movie1Dto.HasValidPrices.Should().BeTrue();
            movie1Dto.CheapestPrice.Should().NotBeNull();
            movie1Dto.CheapestPrice!.Provider.Should().Be("Filmworld");
            movie1Dto.CheapestPrice.Price.Should().Be(14.99m);

            var movie2Dto = result.First(m => m.Title == "Inception");
            movie2Dto.Id.Should().Be(movie2.Id);
            movie2Dto.Title.Should().Be("Inception");
            movie2Dto.Year.Should().Be("2010");
            movie2Dto.Genre.Should().Be("Action, Thriller");
            movie2Dto.Director.Should().Be("Christopher Nolan");
            movie2Dto.Actors.Should().Be("Leonardo DiCaprio");
            movie2Dto.Prices.Should().HaveCount(1);
            movie2Dto.HasValidPrices.Should().BeTrue();
            movie2Dto.CheapestPrice.Should().NotBeNull();
            movie2Dto.CheapestPrice!.Provider.Should().Be("Cinemaworld");
            movie2Dto.CheapestPrice.Price.Should().Be(18.99m);
        }

        [Test]
        public async Task ExecuteAsync_WithMoviesWithoutPrices_ShouldReturnDtosWithNoCheapestPrice()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var movies = new List<Movie> { movie };

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movieDto = result.First();
            movieDto.Title.Should().Be("The Matrix");
            movieDto.Prices.Should().BeEmpty();
            movieDto.HasValidPrices.Should().BeFalse();
            movieDto.CheapestPrice.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMoviesWithInvalidPrices_ShouldReturnDtosWithNoCheapestPrice()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var invalidPrice1 = new MoviePrice(1, "Cinemaworld"); // No price
            var invalidPrice2 = new MoviePrice(1, "Filmworld", 0m); // Zero price

            movie.AddPrice(invalidPrice1);
            movie.AddPrice(invalidPrice2);

            var movies = new List<Movie> { movie };

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movieDto = result.First();
            movieDto.Title.Should().Be("The Matrix");
            movieDto.Prices.Should().HaveCount(2);
            movieDto.HasValidPrices.Should().BeFalse();
            movieDto.CheapestPrice.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyMovieList_ShouldReturnEmptyResult()
        {
            // Arrange
            var movies = new List<Movie>();

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ExecuteAsync_WithMixedValidAndInvalidPrices_ShouldReturnCorrectCheapestPrice()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var invalidPrice = new MoviePrice(1, "Provider1"); // No price
            var validPrice1 = new MoviePrice(1, "Cinemaworld", 15.99m);
            var validPrice2 = new MoviePrice(1, "Filmworld", 12.99m);
            var zeroPrice = new MoviePrice(1, "Provider2", 0m); // Zero price

            movie.AddPrice(invalidPrice);
            movie.AddPrice(validPrice1);
            movie.AddPrice(validPrice2);
            movie.AddPrice(zeroPrice);

            var movies = new List<Movie> { movie };

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movieDto = result.First();
            movieDto.Prices.Should().HaveCount(4);
            movieDto.HasValidPrices.Should().BeTrue();
            movieDto.CheapestPrice.Should().NotBeNull();
            movieDto.CheapestPrice!.Provider.Should().Be("Filmworld");
            movieDto.CheapestPrice.Price.Should().Be(12.99m);
        }

        [Test]
        public async Task ExecuteAsync_ShouldMapAllPriceProperties()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var price = new MoviePrice(1, "Cinemaworld", 15.99m, "AUD");
            price.UpdatePrice(15.99m, "Updated successfully");

            movie.AddPrice(price);
            var movies = new List<Movie> { movie };

            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movieDto = result.First();
            var priceDto = movieDto.Prices.First();

            priceDto.Provider.Should().Be("Cinemaworld");
            priceDto.Price.Should().Be(15.99m);
            priceDto.Currency.Should().Be("AUD");
            priceDto.IsAvailable.Should().BeTrue();
            priceDto.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            priceDto.ErrorMessage.Should().Be("Updated successfully");
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            var movies = new List<Movie>();
            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ReturnsAsync(movies);

            // Act
            await _useCase.ExecuteAsync();

            // Assert
            _mockMovieRepository.Verify(r => r.GetAllWithPricesAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");
            _mockMovieRepository
                .Setup(r => r.GetAllWithPricesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var action = async () => await _useCase.ExecuteAsync();
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }
    }
}
