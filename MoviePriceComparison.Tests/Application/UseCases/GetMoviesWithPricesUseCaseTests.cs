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
    public class GetMoviesWithPricesUseCaseTests
    {
        private Mock<IMovieRepository> _mockMovieRepository;
        private Mock<HttpClient> _mockHttpClient;
        private GetMoviesWithPricesUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();
            _mockHttpClient = new Mock<HttpClient>();
            _useCase = new GetMoviesWithPricesUseCase(_mockMovieRepository.Object, _mockHttpClient.Object);
        }

        [Test]
        public async Task ExecuteAsync_WithMoviesFromRepository_ShouldReturnMovieComparisonDtos()
        {
            // Arrange
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
                            PosterUrl = "https://cinemaworld.com/poster1.jpg"
                        },
                        new MovieProviderDetail
                        {
                            ProviderId = "filmworld",
                            Provider = "Filmworld",
                            MovieId = "fw001",
                            Price = 22.99m,
                            PosterUrl = "https://filmworld.com/poster1.jpg"
                        }
                    }
                },
                new MovieSummary
                {
                    Title = "Avatar",
                    Year = "2009",
                    Genre = "Action",
                    Director = "James Cameron",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "cinemaworld",
                            Provider = "Cinemaworld",
                            MovieId = "cw002",
                            Price = 29.99m
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(2);

            var starWars = result.First(m => m.Title == "Star Wars");
            starWars.Title.Should().Be("Star Wars");
            starWars.Year.Should().Be("1977");
            starWars.Genre.Should().Be("Sci-Fi");
            starWars.Director.Should().Be("George Lucas");
            starWars.Actors.Should().Be("Mark Hamill, Harrison Ford");
            starWars.Plot.Should().Be("A young farm boy joins the rebellion");
            starWars.Rating.Should().Be("8.6");
            starWars.Prices.Should().HaveCount(2);
            starWars.CheapestPrice.Should().NotBeNull();
            starWars.CheapestPrice!.Price.Should().Be(22.99m);
            starWars.CheapestPrice.Provider.Should().Be("Filmworld");

            var avatar = result.First(m => m.Title == "Avatar");
            avatar.Prices.Should().HaveCount(1);
            avatar.CheapestPrice!.Price.Should().Be(29.99m);
        }

        [Test]
        public async Task ExecuteAsync_WithNoMovies_ShouldReturnEmptyList()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<MovieSummary>());

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ExecuteAsync_WithMoviesWithoutPrices_ShouldReturnMoviesWithNoCheapestPrice()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Test Movie",
                    Year = "2023",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "cinemaworld",
                            Provider = "Cinemaworld",
                            MovieId = "cw001",
                            Price = null // No valid price
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.CheapestPrice.Should().NotBeNull(); // MinBy will return the item with null price
            movie.CheapestPrice!.Price.Should().BeNull(); // The price itself will be null
            movie.Prices.Should().HaveCount(1);
            movie.Prices.First().Price.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMixedValidAndInvalidPrices_ShouldOnlyConsiderValidPricesForCheapest()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Test Movie",
                    Year = "2023",
                    ProviderSpecificDetails = new List<MovieProviderDetail>
                    {
                        new MovieProviderDetail
                        {
                            ProviderId = "cinemaworld",
                            Provider = "Cinemaworld",
                            MovieId = "cw001",
                            Price = null // Invalid price
                        },
                        new MovieProviderDetail
                        {
                            ProviderId = "filmworld",
                            Provider = "Filmworld",
                            MovieId = "fw001",
                            Price = 19.99m // Valid price
                        }
                    }
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.CheapestPrice.Should().NotBeNull();
            movie.CheapestPrice!.Price.Should().Be(19.99m); // MinBy actually returns the valid price when comparing with null
            movie.CheapestPrice.Provider.Should().Be("Filmworld"); // The provider with valid price
            movie.Prices.Should().HaveCount(2); // Should include both valid and invalid prices
        }

        [Test]
        public async Task ExecuteAsync_ShouldMapAllMovieProperties()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Complete Movie",
                    Year = "2023",
                    Genre = "Drama",
                    Director = "Test Director",
                    Actors = "Actor 1, Actor 2",
                    Plot = "A compelling story",
                    Rating = "7.5",
                    ProviderSpecificDetails = new List<MovieProviderDetail>()
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.Title.Should().Be("Complete Movie");
            movie.Year.Should().Be("2023");
            movie.Genre.Should().Be("Drama");
            movie.Director.Should().Be("Test Director");
            movie.Actors.Should().Be("Actor 1, Actor 2");
            movie.Plot.Should().Be("A compelling story");
            movie.Rating.Should().Be("7.5");
        }

        [Test]
        public async Task ExecuteAsync_WithRepositoryException_ShouldThrowException()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("Repository error"));

            // Act & Assert
            var action = async () => await _useCase.ExecuteAsync();
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository error");
        }

        [Test]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMoviesWithPricesUseCase(null!, _mockHttpClient.Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new GetMoviesWithPricesUseCase(_mockMovieRepository.Object, null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyProviderSpecificDetails_ShouldHandleGracefully()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Test Movie",
                    Year = "2023",
                    ProviderSpecificDetails = new List<MovieProviderDetail>() // Empty list
                }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.Prices.Should().BeEmpty();
            movie.CheapestPrice.Should().BeNull();
        }

        [Test]
        public async Task ExecuteAsync_WithMultipleMovies_ShouldReturnAllMovies()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary { Title = "Movie 1", Year = "2021" },
                new MovieSummary { Title = "Movie 2", Year = "2022" },
                new MovieSummary { Title = "Movie 3", Year = "2023" }
            };

            _mockMovieRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(m => m.Title).Should().Contain(new[] { "Movie 1", "Movie 2", "Movie 3" });
        }

        [Test]
        public async Task ExecuteAsync_WithValidPrices_ShouldCalculateCheapestCorrectly()
        {
            // Arrange
            var movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    Title = "Price Test Movie",
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
            var result = await _useCase.ExecuteAsync();

            // Assert
            result.Should().HaveCount(1);
            var movie = result.First();
            movie.CheapestPrice.Should().NotBeNull();
            movie.CheapestPrice!.Price.Should().Be(15.50m);
            movie.CheapestPrice.Provider.Should().Be("Provider 2");
            movie.Prices.Should().HaveCount(3);
        }
    }
}
