using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Controllers;
using MoviePriceComparison.Domain.Repositories;
using MoviePriceComparison.Domain.Services;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Controllers
{
    [TestFixture]
    public class MoviesControllerTests
    {
        private Mock<IGetMoviesWithPricesUseCase> _mockGetMoviesUseCase;
        private Mock<IGetMovieDetailUseCase> _mockGetMovieDetailUseCase;
        private Mock<IMovieRepository> _mockMovieRepository;
        private Mock<IApiProviderService> _mockApiProviderService;
        private Mock<ILogger<MoviesController>> _mockLogger;
        private MoviesController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockGetMoviesUseCase = new Mock<IGetMoviesWithPricesUseCase>();
            _mockGetMovieDetailUseCase = new Mock<IGetMovieDetailUseCase>();
            _mockMovieRepository = new Mock<IMovieRepository>();
            _mockApiProviderService = new Mock<IApiProviderService>();
            _mockLogger = new Mock<ILogger<MoviesController>>();

            _controller = new MoviesController(
                _mockGetMoviesUseCase.Object,
                _mockGetMovieDetailUseCase.Object,
                _mockMovieRepository.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task GetMovies_WithValidData_ShouldReturnOkWithMovies()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = "1",
                    Title = "Star Wars",
                    Year = "1977",
                    Genre = "Sci-Fi",
                    Prices = new List<MoviePriceDto>
                    {
                        new MoviePriceDto
                        {
                            ProviderId = "cinemaworld",
                            Provider = "Cinemaworld",
                            MovieId = "cw001",
                            Price = 25.99m,
                            LastUpdated = DateTime.UtcNow
                        }
                    },
                    CheapestPrice = new MoviePriceDto
                    {
                        ProviderId = "cinemaworld",
                        Provider = "Cinemaworld",
                        MovieId = "cw001",
                        Price = 25.99m,
                        LastUpdated = DateTime.UtcNow
                    }
                },
                new MovieComparisonDto
                {
                    Id = "2",
                    Title = "Avatar",
                    Year = "2009",
                    Genre = "Action",
                    Prices = new List<MoviePriceDto>(),
                    CheapestPrice = null
                }
            };

            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(movies);
        }

        [Test]
        public async Task GetMovies_WithNoMovies_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(new List<MovieComparisonDto>());

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var movies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            movies.Should().BeEmpty();
        }

        [Test]
        public async Task GetMovies_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().NotBeNull();
        }

        [Test]
        public async Task GetMovie_WithValidId_ShouldReturnOkWithMovieDetail()
        {
            // Arrange
            var movieId = "test-movie-id";
            var movieDetail = new MovieDetailResponseDto
            {
                Title = "Star Wars",
                Year = "1977",
                Genre = "Sci-Fi",
                Director = "George Lucas",
                Actors = "Mark Hamill, Harrison Ford",
                Plot = "A young farm boy joins the rebellion",
                Rating = "8.6",
                Prices = new List<MoviePriceDto>
                {
                    new MoviePriceDto
                    {
                        ProviderId = "cinemaworld",
                        Provider = "Cinemaworld",
                        MovieId = "cw001",
                        Price = 25.99m,
                        LastUpdated = DateTime.UtcNow
                    }
                },
                CheapestPrice = new MoviePriceDto
                {
                    ProviderId = "cinemaworld",
                    Provider = "Cinemaworld",
                    MovieId = "cw001",
                    Price = 25.99m,
                    LastUpdated = DateTime.UtcNow
                },
                UpdatedAt = DateTime.UtcNow
            };

            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(movieDetail);
        }

        [Test]
        public async Task GetMovie_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var movieId = "non-existent-id";
            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ReturnsAsync((MovieDetailResponseDto?)null);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        [Test]
        public async Task GetMovie_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            var movieId = "test-movie-id";
            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().NotBeNull();
        }

        [Test]
        public async Task GetMovies_ShouldCallUseCaseOnce()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto { Id = "1", Title = "Test Movie" }
            };

            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            await _controller.GetMovies();

            // Assert
            _mockGetMoviesUseCase.Verify(x => x.ExecuteAsync(), Times.Once);
        }

        [Test]
        public async Task GetMovie_ShouldCallUseCaseWithCorrectId()
        {
            // Arrange
            var movieId = "test-movie-id";
            var movieDetail = new MovieDetailResponseDto { Title = "Test Movie" };

            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ReturnsAsync(movieDetail);

            // Act
            await _controller.GetMovie(movieId);

            // Assert
            _mockGetMovieDetailUseCase.Verify(x => x.ExecuteAsync(movieId), Times.Once);
        }

        [Test]
        public async Task GetMovies_WithException_ShouldLogError()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetMovies();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting movies")),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetMovie_WithException_ShouldLogErrorWithMovieId()
        {
            // Arrange
            var movieId = "test-movie-id";
            var exception = new InvalidOperationException("Test exception");
            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetMovie(movieId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error getting movie with ID {movieId}")),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void Constructor_WithNullGetMoviesUseCase_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MoviesController(
                null!,
                _mockGetMovieDetailUseCase.Object,
                _mockMovieRepository.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("getMoviesWithPricesUseCase");
        }

        [Test]
        public void Constructor_WithNullGetMovieDetailUseCase_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MoviesController(
                _mockGetMoviesUseCase.Object,
                null!,
                _mockMovieRepository.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("getMovieDetailUseCase");
        }

        [Test]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MoviesController(
                _mockGetMoviesUseCase.Object,
                _mockGetMovieDetailUseCase.Object,
                _mockMovieRepository.Object,
                _mockApiProviderService.Object,
                null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Test]
        public async Task GetMovies_WithMultipleMovies_ShouldReturnAllMovies()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto { Id = "1", Title = "Movie 1" },
                new MovieComparisonDto { Id = "2", Title = "Movie 2" },
                new MovieComparisonDto { Id = "3", Title = "Movie 3" }
            };

            _mockGetMoviesUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            returnedMovies.Should().HaveCount(3);
            returnedMovies!.Select(m => m.Title).Should().Contain(new[] { "Movie 1", "Movie 2", "Movie 3" });
        }

        [Test]
        public async Task GetMovie_WithValidMovieData_ShouldReturnCompleteMovieInformation()
        {
            // Arrange
            var movieId = "complete-movie-id";
            var movieDetail = new MovieDetailResponseDto
            {
                Title = "Complete Movie",
                Year = "2023",
                Type = "movie",
                Rated = "PG-13",
                Released = "2023-01-01",
                Runtime = "120 min",
                Genre = "Action, Drama",
                Director = "Test Director",
                Writer = "Test Writer",
                Actors = "Actor 1, Actor 2",
                Plot = "A compelling story",
                Language = "English",
                Country = "USA",
                Awards = "Best Picture",
                Poster = "https://example.com/poster.jpg",
                Metascore = "85",
                Rating = "8.5",
                Votes = "100,000",
                Prices = new List<MoviePriceDto>
                {
                    new MoviePriceDto
                    {
                        ProviderId = "provider1",
                        Provider = "Provider 1",
                        MovieId = "p1001",
                        Price = 19.99m,
                        LastUpdated = DateTime.UtcNow
                    }
                },
                CheapestPrice = new MoviePriceDto
                {
                    ProviderId = "provider1",
                    Provider = "Provider 1",
                    MovieId = "p1001",
                    Price = 19.99m,
                    LastUpdated = DateTime.UtcNow
                },
                UpdatedAt = DateTime.UtcNow
            };

            _mockGetMovieDetailUseCase.Setup(x => x.ExecuteAsync(movieId))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedMovie = okResult!.Value as MovieDetailResponseDto;
            returnedMovie.Should().NotBeNull();
            returnedMovie!.Title.Should().Be("Complete Movie");
            returnedMovie.Genre.Should().Be("Action, Drama");
            returnedMovie.Prices.Should().HaveCount(1);
            returnedMovie.CheapestPrice.Should().NotBeNull();
        }
    }
}
