using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Domain.Services;
using MoviePriceComparison.Infrastructure.Repositories;
using MoviePriceComparison.Infrastructure.Services;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Infrastructure.Repositories
{
    [TestFixture]
    public class MovieRepositoryTests
    {
        private Mock<IExternalMovieApiService> _mockExternalMovieApiService;
        private Mock<IApiProviderService> _mockApiProviderService;
        private Mock<ILogger<MovieRepository>> _mockLogger;
        private IMemoryCache _memoryCache;
        private IOptions<ExternalMovieApiCacheSettings> _cacheSettings;
        private MovieRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _mockExternalMovieApiService = new Mock<IExternalMovieApiService>();
            _mockApiProviderService = new Mock<IApiProviderService>();
            _mockLogger = new Mock<ILogger<MovieRepository>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            var cacheSettings = new ExternalMovieApiCacheSettings
            {
                CacheDurationMinutes = 5,
                MaxAgeMinutes = 10
            };
            _cacheSettings = Options.Create(cacheSettings);

            _repository = new MovieRepository(
                _memoryCache,
                _mockExternalMovieApiService.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object,
                _cacheSettings);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache.Dispose();
        }

        [Test]
        public void Constructor_WithNullCache_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(
                null!,
                _mockExternalMovieApiService.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object,
                _cacheSettings);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("cache");
        }

        [Test]
        public void Constructor_WithNullExternalMovieApiService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(
                _memoryCache,
                null!,
                _mockApiProviderService.Object,
                _mockLogger.Object,
                _cacheSettings);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("externalMovieApiService");
        }

        [Test]
        public void Constructor_WithNullApiProviderService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(
                _memoryCache,
                _mockExternalMovieApiService.Object,
                null!,
                _mockLogger.Object,
                _cacheSettings);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("apiProviderService");
        }

        [Test]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(
                _memoryCache,
                _mockExternalMovieApiService.Object,
                _mockApiProviderService.Object,
                null!,
                _cacheSettings);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Test]
        public void Constructor_WithNullCacheSettings_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(
                _memoryCache,
                _mockExternalMovieApiService.Object,
                _mockApiProviderService.Object,
                _mockLogger.Object,
                null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("cacheSettings");
        }

        [Test]
        public async Task GetAllAsync_WithCachedData_ShouldReturnCachedMovies()
        {
            // Arrange
            var cachedMovies = new List<MovieSummary>
            {
                new MovieSummary { Title = "Cached Movie 1" },
                new MovieSummary { Title = "Cached Movie 2" }
            };

            _memoryCache.Set("movies_list", cachedMovies);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(cachedMovies);

            // Verify external services were not called
            _mockApiProviderService.Verify(x => x.GetApiProvidersAsync(), Times.Never);
            _mockExternalMovieApiService.Verify(x => x.GetMoviesFromProviderAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetAllAsync_WithoutCachedData_ShouldFetchFromExternalAPIs()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" },
                new ApiProvider { Id = "filmworld", IsEnabled = true, DisplayName = "Filmworld" }
            };

            var cinemaworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "cw001", Type = "movie" }
            };

            var filmworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "fw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Genre = "Sci-Fi",
                Director = "George Lucas",
                Price = "25.99",
                Poster = "https://example.com/poster.jpg"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(cinemaworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("filmworld"))
                .ReturnsAsync(filmworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(movieDetail);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("filmworld", "fw001"))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.Title.Should().Be("Star Wars");
            movie.Year.Should().Be("1977");
            movie.Genre.Should().Be("Sci-Fi");
            movie.Director.Should().Be("George Lucas");
            movie.ProviderSpecificDetails.Should().HaveCount(2);

            // Verify data was cached
            _memoryCache.TryGetValue("movies_list", out var cachedResult).Should().BeTrue();
            cachedResult.Should().BeEquivalentTo(result);
        }

        [Test]
        public async Task GetAllAsync_WithDisabledProvider_ShouldSkipProvider()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" },
                new ApiProvider { Id = "filmworld", IsEnabled = false, DisplayName = "Filmworld" }
            };

            var cinemaworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "cw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Price = "25.99"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(cinemaworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.ProviderSpecificDetails.Should().HaveCount(1);
            movie.ProviderSpecificDetails.First().ProviderId.Should().Be("cinemaworld");

            // Verify filmworld was not called
            _mockExternalMovieApiService.Verify(x => x.GetMoviesFromProviderAsync("filmworld"), Times.Never);
        }

        [Test]
        public async Task GetAllAsync_WithProviderError_ShouldContinueWithOtherProviders()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" },
                new ApiProvider { Id = "filmworld", IsEnabled = true, DisplayName = "Filmworld" }
            };

            var filmworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "fw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Price = "25.99"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ThrowsAsync(new HttpRequestException("API Error"));

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("filmworld"))
                .ReturnsAsync(filmworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("filmworld", "fw001"))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.ProviderSpecificDetails.Should().HaveCount(1);
            movie.ProviderSpecificDetails.First().ProviderId.Should().Be("filmworld");

            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching movies from provider cinemaworld")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetAllAsync_WithSameMovieFromMultipleProviders_ShouldMergeMovieData()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" },
                new ApiProvider { Id = "filmworld", IsEnabled = true, DisplayName = "Filmworld" }
            };

            var cinemaworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "cw001", Type = "movie" }
            };

            var filmworldMovies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "fw001", Type = "movie" }
            };

            var cinemaworldDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Price = "30.99",
                Poster = "https://cinemaworld.com/poster.jpg"
            };

            var filmworldDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Price = "25.99",
                Poster = "https://filmworld.com/poster.jpg"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(cinemaworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("filmworld"))
                .ReturnsAsync(filmworldMovies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(cinemaworldDetail);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("filmworld", "fw001"))
                .ReturnsAsync(filmworldDetail);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.Title.Should().Be("Star Wars");
            movie.ProviderSpecificDetails.Should().HaveCount(2);

            var cinemaworldProvider = movie.ProviderSpecificDetails.First(p => p.ProviderId == "cinemaworld");
            cinemaworldProvider.Price.Should().Be(30.99m);
            cinemaworldProvider.PosterUrl.Should().Be("https://cinemaworld.com/poster.jpg");

            var filmworldProvider = movie.ProviderSpecificDetails.First(p => p.ProviderId == "filmworld");
            filmworldProvider.Price.Should().Be(25.99m);
            filmworldProvider.PosterUrl.Should().Be("https://filmworld.com/poster.jpg");
        }

        [Test]
        public async Task GetAllAsync_WithInvalidPriceFormat_ShouldHandleGracefully()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" }
            };

            var movies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Star Wars", Year = "1977", ID = "cw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Star Wars",
                Year = "1977",
                Price = "invalid_price"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(movies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.ProviderSpecificDetails.Should().HaveCount(1);
            movie.ProviderSpecificDetails.First().Price.Should().BeNull(); // Default value when parsing fails
        }

        [Test]
        public async Task RefreshData_ShouldClearCacheAndRefetchData()
        {
            // Arrange
            var cachedMovies = new List<MovieSummary>
            {
                new MovieSummary { Title = "Cached Movie" }
            };
            _memoryCache.Set("movies_list", cachedMovies);

            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" }
            };

            var movies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Fresh Movie", Year = "2023", ID = "cw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Fresh Movie",
                Year = "2023",
                Price = "15.99"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockApiProviderService.Setup(x => x.RefreshApiProvidersAsync())
                .Returns(Task.CompletedTask);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(movies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(movieDetail);

            // Act
            await _repository.RefreshData();

            // Assert
            // Verify cache was cleared and new data was fetched
            _memoryCache.TryGetValue("movies_list", out var newCachedResult).Should().BeTrue();
            var newMovies = (IEnumerable<MovieSummary>)newCachedResult!;
            newMovies.Should().HaveCount(1);
            newMovies.First().Title.Should().Be("Fresh Movie");

            _mockApiProviderService.Verify(x => x.RefreshApiProvidersAsync(), Times.Once);
        }

        [Test]
        public async Task GetAllAsync_ShouldSetCorrectCacheOptions()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "cinemaworld", IsEnabled = true, DisplayName = "Cinemaworld" }
            };

            var movies = new List<ExternalMovieSummaryDto>
            {
                new ExternalMovieSummaryDto { Title = "Test Movie", Year = "2023", ID = "cw001", Type = "movie" }
            };

            var movieDetail = new ExternalMovieDetailDto
            {
                Title = "Test Movie",
                Year = "2023",
                Price = "15.99"
            };

            _mockApiProviderService.Setup(x => x.GetApiProvidersAsync())
                .ReturnsAsync(providers);

            _mockExternalMovieApiService.Setup(x => x.GetMoviesFromProviderAsync("cinemaworld"))
                .ReturnsAsync(movies);

            _mockExternalMovieApiService.Setup(x => x.GetMovieDetailsFromProviderAsync("cinemaworld", "cw001"))
                .ReturnsAsync(movieDetail);

            // Act
            await _repository.GetAllAsync();

            // Assert
            _memoryCache.TryGetValue("movies_list", out var cachedResult).Should().BeTrue();

            // Verify logging occurred
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache miss - fetching movies from external APIs")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cached 1 movies for 5 minutes")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
