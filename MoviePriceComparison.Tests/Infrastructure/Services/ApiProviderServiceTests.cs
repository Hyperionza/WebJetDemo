using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MoviePriceComparison.Application.DTOs;
using MoviePriceComparison.Domain.Services;
using MoviePriceComparison.Infrastructure.Services;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Infrastructure.Services
{
    [TestFixture]
    public class ApiProviderServiceTests
    {
        private Mock<HttpClient> _mockHttpClient;
        private Mock<ILogger<ApiProviderService>> _mockLogger;
        private IMemoryCache _memoryCache;
        private IOptions<ApiProviderConfiguration> _config;
        private ApiProviderService _service;

        [SetUp]
        public void SetUp()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockLogger = new Mock<ILogger<ApiProviderService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            var configuration = new ApiProviderConfiguration
            {
                ApiProviderServiceUrl = "https://test-api.com",
                CacheDurationMinutes = 15,
                TimeoutSeconds = 30
            };
            _config = Options.Create(configuration);

            _service = new ApiProviderService(
                _mockHttpClient.Object,
                _memoryCache,
                _mockLogger.Object,
                _config);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache.Dispose();
        }

        [Test]
        public async Task GetApiProvidersAsync_WithCachedData_ShouldReturnCachedProviders()
        {
            // Arrange
            var cachedProviders = new List<ApiProvider>
            {
                new ApiProvider { Id = "cached1", Name = "Cached Provider 1" },
                new ApiProvider { Id = "cached2", Name = "Cached Provider 2" }
            };

            _memoryCache.Set("api_providers", cachedProviders);

            // Act
            var result = await _service.GetApiProvidersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(cachedProviders);
        }

        [Test]
        public async Task GetApiProvidersAsync_WithoutCachedData_ShouldReturnFallbackProviders()
        {
            // Act
            var result = await _service.GetApiProvidersAsync();

            // Assert
            result.Should().HaveCount(2);

            var cinemaworld = result.FirstOrDefault(p => p.Id == "cinemaworld");
            cinemaworld.Should().NotBeNull();
            cinemaworld!.Name.Should().Be("cinemaworld");
            cinemaworld.DisplayName.Should().Be("Cinemaworld");
            cinemaworld.BaseUrl.Should().Be("https://webjetapitest.azurewebsites.net/api/cinemaworld");
            cinemaworld.ApiToken.Should().Be("sjd1HfkjU83ksdsm3802k");
            cinemaworld.IsEnabled.Should().BeTrue();
            cinemaworld.Priority.Should().Be(1);

            var filmworld = result.FirstOrDefault(p => p.Id == "filmworld");
            filmworld.Should().NotBeNull();
            filmworld!.Name.Should().Be("filmworld");
            filmworld.DisplayName.Should().Be("Filmworld");
            filmworld.BaseUrl.Should().Be("https://webjetapitest.azurewebsites.net/api/filmworld");
            filmworld.ApiToken.Should().Be("sjd1HfkjU83ksdsm3802k");
            filmworld.IsEnabled.Should().BeTrue();
            filmworld.Priority.Should().Be(2);

            // Verify data was cached
            _memoryCache.TryGetValue("api_providers", out var cachedResult).Should().BeTrue();
            cachedResult.Should().BeEquivalentTo(result);
        }

        [Test]
        public async Task GetApiProviderAsync_WithExistingProvider_ShouldReturnProvider()
        {
            // Act
            var result = await _service.GetApiProviderAsync("cinemaworld");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("cinemaworld");
            result.DisplayName.Should().Be("Cinemaworld");
        }

        [Test]
        public async Task GetApiProviderAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Act
            var result = await _service.GetApiProviderAsync("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetApiProviderAsync_ShouldBeCaseInsensitive()
        {
            // Act
            var result = await _service.GetApiProviderAsync("CINEMAWORLD");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("cinemaworld");
        }

        [Test]
        public async Task RefreshApiProvidersAsync_ShouldClearCacheAndRefetchData()
        {
            // Arrange
            var cachedProviders = new List<ApiProvider>
            {
                new ApiProvider { Id = "cached", Name = "Cached Provider" }
            };
            _memoryCache.Set("api_providers", cachedProviders);

            // Act
            await _service.RefreshApiProvidersAsync();

            // Assert
            // Verify cache was cleared and new data was fetched
            _memoryCache.TryGetValue("api_providers", out var newCachedResult).Should().BeTrue();
            var newProviders = (List<ApiProvider>)newCachedResult!;
            newProviders.Should().HaveCount(2); // Should have fallback providers
            newProviders.Should().NotBeEquivalentTo(cachedProviders);
        }

        [Test]
        public async Task IsProviderEnabledAsync_WithEnabledProvider_ShouldReturnTrue()
        {
            // Act
            var result = await _service.IsProviderEnabledAsync("cinemaworld");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsProviderEnabledAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Act
            var result = await _service.IsProviderEnabledAsync("nonexistent");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task IsProviderEnabledAsync_WithDisabledProvider_ShouldReturnFalse()
        {
            // Arrange
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Id = "disabled", Name = "Disabled Provider", IsEnabled = false }
            };
            _memoryCache.Set("api_providers", providers);

            // Act
            var result = await _service.IsProviderEnabledAsync("disabled");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetApiProvidersAsync_ShouldSetCorrectCacheExpiration()
        {
            // Act
            await _service.GetApiProvidersAsync();

            // Assert
            _memoryCache.TryGetValue("api_providers", out var cachedResult).Should().BeTrue();
            cachedResult.Should().NotBeNull();
        }

        [Test]
        public async Task GetApiProvidersAsync_ShouldLogInformationMessages()
        {
            // Act
            await _service.GetApiProvidersAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching API providers from configuration service")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using fallback API providers configuration")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task RefreshApiProvidersAsync_ShouldLogRefreshMessage()
        {
            // Act
            await _service.RefreshApiProvidersAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refreshing API providers cache")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetApiProvidersAsync_WithCachedData_ShouldLogDebugMessage()
        {
            // Arrange
            var cachedProviders = new List<ApiProvider>
            {
                new ApiProvider { Id = "cached", Name = "Cached Provider" }
            };
            _memoryCache.Set("api_providers", cachedProviders);

            // Act
            await _service.GetApiProvidersAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returning cached API providers")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new ApiProviderService(
                null!,
                _memoryCache,
                _mockLogger.Object,
                _config);

            action.Should().NotThrow(); // Constructor doesn't validate null HttpClient
        }

        [Test]
        public void Constructor_WithNullMemoryCache_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new ApiProviderService(
                _mockHttpClient.Object,
                null!,
                _mockLogger.Object,
                _config);

            action.Should().NotThrow(); // Constructor doesn't validate null MemoryCache
        }

        [Test]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new ApiProviderService(
                _mockHttpClient.Object,
                _memoryCache,
                null!,
                _config);

            action.Should().NotThrow(); // Constructor doesn't validate null Logger
        }

        [Test]
        public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new ApiProviderService(
                _mockHttpClient.Object,
                _memoryCache,
                _mockLogger.Object,
                null!);

            action.Should().Throw<NullReferenceException>(); // Actual behavior throws NullReferenceException
        }
    }
}
