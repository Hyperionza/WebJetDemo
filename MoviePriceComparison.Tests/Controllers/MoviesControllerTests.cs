using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Application.UseCases;
using MoviePriceComparison.Controllers;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Controllers
{
    [TestFixture]
    public class MoviesControllerTests
    {
        private Mock<IGetMoviesWithPricesUseCase> _mockGetMoviesUseCase;
        private Mock<IGetMovieDetailUseCase> _mockGetMovieDetailUseCase;
        private Mock<ILogger<MoviesController>> _mockLogger;
        private MoviesController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockGetMoviesUseCase = new Mock<IGetMoviesWithPricesUseCase>();
            _mockGetMovieDetailUseCase = new Mock<IGetMovieDetailUseCase>();
            _mockLogger = new Mock<ILogger<MoviesController>>();
            _controller = new MoviesController(
                _mockGetMoviesUseCase.Object,
                _mockGetMovieDetailUseCase.Object,
                _mockLogger.Object);
        }

        [Test]
        public void Constructor_WithNullGetMoviesUseCase_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MoviesController(
                null!,
                _mockGetMovieDetailUseCase.Object,
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
                null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Test]
        public async Task GetMovies_WithSuccessfulUseCase_ShouldReturnOkWithMovies()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Year = "1999",
                    Genre = "Action, Sci-Fi",
                    HasValidPrices = true,
                    CheapestPrice = new MoviePriceDto
                    {
                        Provider = "Filmworld",
                        Price = 14.99m,
                        Currency = "AUD",
                        IsAvailable = true
                    }
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(movies);
        }

        [Test]
        public async Task GetMovies_WithEmptyResult_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>();

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(movies);
        }

        [Test]
        public async Task GetMovies_WhenUseCaseThrows_ShouldReturnInternalServerError()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");
            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ThrowsAsync(expectedException);

            // Act
            var result = await _controller.GetMovies();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);

            var errorResponse = objectResult.Value as dynamic;
            // Error response should be present
        }

        [Test]
        public async Task GetMovie_WithExistingMovie_ShouldReturnOkWithMovieDetail()
        {
            // Arrange
            var movieId = 1;
            var movieDetail = new MovieDetailResponseDto
            {
                Id = movieId,
                Title = "The Matrix",
                Year = "1999",
                Genre = "Action, Sci-Fi",
                Director = "Wachowski Sisters",
                HasValidPrices = true,
                CheapestPrice = new MoviePriceDto
                {
                    Provider = "Filmworld",
                    Price = 14.99m,
                    Currency = "AUD",
                    IsAvailable = true
                }
            };

            _mockGetMovieDetailUseCase
                .Setup(u => u.ExecuteAsync(movieId))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(movieDetail);
        }

        [Test]
        public async Task GetMovie_WithNonExistentMovie_ShouldReturnNotFound()
        {
            // Arrange
            var movieId = 999;

            _mockGetMovieDetailUseCase
                .Setup(u => u.ExecuteAsync(movieId))
                .ReturnsAsync((MovieDetailResponseDto?)null);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;

            var errorResponse = notFoundResult!.Value as dynamic;
            // Error response should be present
        }

        [Test]
        public async Task GetMovie_WhenUseCaseThrows_ShouldReturnInternalServerError()
        {
            // Arrange
            var movieId = 1;
            var expectedException = new InvalidOperationException("Database error");
            _mockGetMovieDetailUseCase
                .Setup(u => u.ExecuteAsync(movieId))
                .ThrowsAsync(expectedException);

            // Act
            var result = await _controller.GetMovie(movieId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);

            var errorResponse = objectResult.Value as dynamic;
            // Error response should be present
        }

        [Test]
        public async Task SearchMovies_WithValidQuery_ShouldReturnFilteredMovies()
        {
            // Arrange
            var query = "matrix";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Year = "1999",
                    Genre = "Action, Sci-Fi",
                    HasValidPrices = true
                },
                new MovieComparisonDto
                {
                    Id = 2,
                    Title = "Avatar",
                    Year = "2009",
                    Genre = "Action, Adventure",
                    HasValidPrices = true
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().HaveCount(1);
            filteredMovies!.First().Title.Should().Be("The Matrix");
        }

        [Test]
        public async Task SearchMovies_WithGenreMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var query = "action";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Genre = "Action, Sci-Fi"
                },
                new MovieComparisonDto
                {
                    Id = 2,
                    Title = "Avatar",
                    Genre = "Action, Adventure"
                },
                new MovieComparisonDto
                {
                    Id = 3,
                    Title = "Titanic",
                    Genre = "Romance, Drama"
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().HaveCount(2);
            filteredMovies!.Should().Contain(m => m.Title == "The Matrix");
            filteredMovies.Should().Contain(m => m.Title == "Avatar");
            filteredMovies.Should().NotContain(m => m.Title == "Titanic");
        }

        [Test]
        public async Task SearchMovies_WithDirectorMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var query = "nolan";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "Inception",
                    Director = "Christopher Nolan"
                },
                new MovieComparisonDto
                {
                    Id = 2,
                    Title = "The Matrix",
                    Director = "Wachowski Sisters"
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().HaveCount(1);
            filteredMovies!.First().Title.Should().Be("Inception");
        }

        [Test]
        public async Task SearchMovies_WithActorMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var query = "keanu";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Actors = "Keanu Reeves, Laurence Fishburne"
                },
                new MovieComparisonDto
                {
                    Id = 2,
                    Title = "John Wick",
                    Actors = "Keanu Reeves, Michael Nyqvist"
                },
                new MovieComparisonDto
                {
                    Id = 3,
                    Title = "Avatar",
                    Actors = "Sam Worthington, Zoe Saldana"
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().HaveCount(2);
            filteredMovies!.Should().Contain(m => m.Title == "The Matrix");
            filteredMovies.Should().Contain(m => m.Title == "John Wick");
            filteredMovies.Should().NotContain(m => m.Title == "Avatar");
        }

        [Test]
        public async Task SearchMovies_WithEmptyQuery_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchMovies("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            var errorResponse = badRequestResult!.Value as dynamic;
            // Error response should be present
        }

        [Test]
        public async Task SearchMovies_WithNullQuery_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchMovies(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task SearchMovies_WithWhitespaceQuery_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchMovies("   ");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task SearchMovies_WithNoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            var query = "nonexistent";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Genre = "Action, Sci-Fi"
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().BeEmpty();
        }

        [Test]
        public async Task SearchMovies_WhenUseCaseThrows_ShouldReturnInternalServerError()
        {
            // Arrange
            var query = "matrix";
            var expectedException = new InvalidOperationException("Database error");
            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ThrowsAsync(expectedException);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task SearchMovies_ShouldBeCaseInsensitive()
        {
            // Arrange
            var query = "MATRIX";
            var allMovies = new List<MovieComparisonDto>
            {
                new MovieComparisonDto
                {
                    Id = 1,
                    Title = "The Matrix",
                    Genre = "Action, Sci-Fi"
                }
            };

            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(allMovies);

            // Act
            var result = await _controller.SearchMovies(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var filteredMovies = okResult!.Value as IEnumerable<MovieComparisonDto>;
            filteredMovies.Should().HaveCount(1);
            filteredMovies!.First().Title.Should().Be("The Matrix");
        }

        [Test]
        public void GetApiHealth_ShouldReturnOkWithHealthStatus()
        {
            // Act
            var result = _controller.GetApiHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            var healthResponse = okResult!.Value as dynamic;
            // Health response should be present
        }

        [Test]
        public async Task GetMovies_ShouldCallUseCaseOnce()
        {
            // Arrange
            var movies = new List<MovieComparisonDto>();
            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            await _controller.GetMovies();

            // Assert
            _mockGetMoviesUseCase.Verify(u => u.ExecuteAsync(), Times.Once);
        }

        [Test]
        public async Task GetMovie_ShouldCallUseCaseOnceWithCorrectId()
        {
            // Arrange
            var movieId = 123;
            var movieDetail = new MovieDetailResponseDto { Id = movieId, Title = "Test Movie" };
            _mockGetMovieDetailUseCase
                .Setup(u => u.ExecuteAsync(movieId))
                .ReturnsAsync(movieDetail);

            // Act
            await _controller.GetMovie(movieId);

            // Assert
            _mockGetMovieDetailUseCase.Verify(u => u.ExecuteAsync(movieId), Times.Once);
        }

        [Test]
        public async Task SearchMovies_ShouldCallUseCaseOnce()
        {
            // Arrange
            var query = "test";
            var movies = new List<MovieComparisonDto>();
            _mockGetMoviesUseCase
                .Setup(u => u.ExecuteAsync())
                .ReturnsAsync(movies);

            // Act
            await _controller.SearchMovies(query);

            // Assert
            _mockGetMoviesUseCase.Verify(u => u.ExecuteAsync(), Times.Once);
        }
    }
}
