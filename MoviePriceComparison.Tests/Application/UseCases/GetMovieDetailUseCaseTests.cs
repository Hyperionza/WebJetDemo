using FluentAssertions;
using Moq;
using MoviePriceComparison.Application.DTOs;
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
        private Mock<HttpClient> _mockHttpClient;
        private GetMovieDetailUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();
            _mockHttpClient = new Mock<HttpClient>();
            _useCase = new GetMovieDetailUseCase(_mockMovieRepository.Object, _mockHttpClient.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithValidMovieId_ShouldReturnMovieDetail()
        {
            // Arrange
            var movieId = "cw001"; // Use actual MovieId from ProviderSpecificDetails
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Star Wars",
                    Year = "1977",
                    Genre = "Sci-Fi",
                    Director = "George Lucas",
                    Actors = "Mark Hamill, Harrison Ford",
                    Plot = "A young farm boy joins the rebellion",
                    Rating = "8.6",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "cinemaworld",
                            Provider = "Cinemaworld",
                            MovieId = "cw001",
                            Price = 25.99m,
                            PosterUrl = "https://cinemaworld.com/poster.jpg"
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Star Wars");
            result.Year.Should().Be("1977");
            result.Genre.Should().Be("Sci-Fi");
            result.Director.Should().Be("George Lucas");
            result.Actors.Should().Be("Mark Hamill, Harrison Ford");
            result.Plot.Should().Be("A young farm boy joins the rebellion");
            result.Rating.Should().Be("8.6");
            result.Prices.Should().HaveCount(1);
            result.CheapestPrice.Should().NotBeNull();
            result.CheapestPrice!.Price.Should().Be(25.99m);
        }

        [Test]
        public async Task ExecuteAsync_WithNonExistentMovieId_ShouldReturnNull()
        {
            // Arrange
            var movieId = "non-existent-id";
            var movies = new List<MovieSummary>
            {
                new MovieSummary { Title = "Different Movie" }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyMovieList_ShouldReturnNull()
        {
            // Arrange
            var movieId = "any-id";
            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<MovieSummary>());

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMultiplePrices_ShouldCalculateCheapestCorrectly()
        {
            // Arrange
            var movieId = "p2001"; // Use actual MovieId from ProviderSpecificDetails
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Test Movie",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "provider1",
                            Provider = "Provider 1",
                            MovieId = "p1001",
                            Price = 30.00m
                        },
                        new MovieProviderDetail
                        {
                            ProviderId = "provider2",
                            Provider = "Provider 2",
                            MovieId = "p2001",
                            Price = 15.50m // This should be cheapest
                        },
                        new MovieProviderDetail
                        {
                            ProviderId = "provider3",
                            Provider = "Provider 3",
                            MovieId = "p3001",
                            Price = 25.99m
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.CheapestPrice.Should().NotBeNull();
            result.CheapestPrice!.Price.Should().Be(15.50m);
            result.CheapestPrice.Provider.Should().Be("Provider 2");
            result.Prices.Should().HaveCount(3);
        }

        [Test]
        public async Task ExecuteAsync_WithNullPrices_ShouldHandleGracefully()
        {
            // Arrange
            var movieId = "p1001"; // Use actual MovieId from ProviderSpecificDetails
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Test Movie",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "provider1",
                            Provider = "Provider 1",
                            MovieId = "p1001",
                            Price = null
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.CheapestPrice.Should().NotBeNull(); // MinBy returns the item with null price
            result.CheapestPrice!.Price.Should().BeNull(); // The price itself is null
            result.Prices.Should().HaveCount(1);
            result.Prices.First().Price.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithRepositoryException_ShouldThrowException()
        {
            // Arrange
            var movieId = "test-movie-id";
            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("Repository error"));

            // Act & Assert
            var action = async () => await _useCase.ExecuteAsync(movieId);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository error");
        }

        [Test]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMovieDetailUseCase(null!, _mockHttpClient.Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMovieDetailUseCase(_mockMovieRepository.Object, null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteAsync_ShouldMapAllMovieProperties()
        {
            // Arrange
            var movieId = "complete-movie-id";
            var movies = new List<MovieSummary>
            {
                new MovieSummary
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
                    Metascore = "85",
                    Rating = "8.5",
                    Votes = "100,000",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "provider1",
                            Provider = "Provider 1",
                            MovieId = "complete-movie-id",
                            Price = 19.99m
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Complete Movie");
            result.Year.Should().Be("2023");
            result.Type.Should().Be("movie");
            result.Rated.Should().Be("PG-13");
            result.Released.Should().Be("2023-01-01");
            result.Runtime.Should().Be("120 min");
            result.Genre.Should().Be("Action, Drama");
            result.Director.Should().Be("Test Director");
            result.Writer.Should().Be("Test Writer");
            result.Actors.Should().Be("Actor 1, Actor 2");
            result.Plot.Should().Be("A compelling story");
            result.Language.Should().Be("English");
            result.Country.Should().Be("USA");
            result.Awards.Should().Be("Best Picture");
            result.Metascore.Should().Be("85");
            result.Rating.Should().Be("8.5");
            result.Votes.Should().Be("100,000");
        }
    }
}
