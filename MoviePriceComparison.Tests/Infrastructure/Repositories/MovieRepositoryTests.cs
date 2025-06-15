using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MoviePriceComparison.Domain.Entities;
using MoviePriceComparison.Infrastructure.Data;
using MoviePriceComparison.Infrastructure.Repositories;
using NUnit.Framework;

namespace MoviePriceComparison.Tests.Infrastructure.Repositories
{
    [TestFixture]
    public class MovieRepositoryTests
    {
        private MovieDbContext _context;
        private MovieRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<MovieDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MovieDbContext(options);
            _repository = new MovieRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new MovieRepository(null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }

        [Test]
        public async Task GetAllAsync_WithMoviesInDatabase_ShouldReturnAllMoviesOrderedByTitle()
        {
            // Arrange
            var movie1 = new Movie("Zulu", "1964", "movie");
            var movie2 = new Movie("Avatar", "2009", "movie");
            var movie3 = new Movie("Matrix", "1999", "movie");

            _context.Movies.AddRange(movie1, movie2, movie3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            var movieList = result.ToList();
            movieList[0].Title.Should().Be("Avatar");
            movieList[1].Title.Should().Be("Matrix");
            movieList[2].Title.Should().Be("Zulu");
        }

        [Test]
        public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetByIdAsync_WithExistingMovie_ShouldReturnMovie()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(movie.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("The Matrix");
            result.Year.Should().Be("1999");
        }

        [Test]
        public async Task GetByIdAsync_WithNonExistentMovie_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByIdWithPricesAsync_WithExistingMovie_ShouldReturnMovieWithPrices()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var price1 = new MoviePrice(movie.Id, "Cinemaworld", 15.99m);
            var price2 = new MoviePrice(movie.Id, "Filmworld", 14.99m);

            movie.AddPrice(price1);
            movie.AddPrice(price2);

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithPricesAsync(movie.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("The Matrix");
            result.MoviePrices.Should().HaveCount(2);
            result.MoviePrices.Should().Contain(p => p.Provider == "Cinemaworld" && p.Price == 15.99m);
            result.MoviePrices.Should().Contain(p => p.Provider == "Filmworld" && p.Price == 14.99m);
        }

        [Test]
        public async Task GetByIdWithPricesAsync_WithNonExistentMovie_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdWithPricesAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetAllWithPricesAsync_ShouldReturnAllMoviesWithPricesOrderedByTitle()
        {
            // Arrange
            var movie1 = new Movie("Zulu", "1964", "movie");
            var movie2 = new Movie("Avatar", "2009", "movie");
            var price1 = new MoviePrice(movie1.Id, "Cinemaworld", 12.99m);
            var price2 = new MoviePrice(movie2.Id, "Filmworld", 18.99m);

            movie1.AddPrice(price1);
            movie2.AddPrice(price2);

            _context.Movies.AddRange(movie1, movie2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllWithPricesAsync();

            // Assert
            result.Should().HaveCount(2);
            var movieList = result.ToList();
            movieList[0].Title.Should().Be("Avatar");
            movieList[0].MoviePrices.Should().HaveCount(1);
            movieList[1].Title.Should().Be("Zulu");
            movieList[1].MoviePrices.Should().HaveCount(1);
        }

        [Test]
        public async Task SearchAsync_WithTitleMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            var movie2 = new Movie("Matrix Reloaded", "2003", "movie");
            var movie3 = new Movie("Avatar", "2009", "movie");

            _context.Movies.AddRange(movie1, movie2, movie3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("matrix");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Title == "The Matrix");
            result.Should().Contain(m => m.Title == "Matrix Reloaded");
            result.Should().NotContain(m => m.Title == "Avatar");
        }

        [Test]
        public async Task SearchAsync_WithGenreMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            movie1.UpdateDetails(genre: "Action, Sci-Fi");
            var movie2 = new Movie("Avatar", "2009", "movie");
            movie2.UpdateDetails(genre: "Action, Adventure");
            var movie3 = new Movie("Titanic", "1997", "movie");
            movie3.UpdateDetails(genre: "Romance, Drama");

            _context.Movies.AddRange(movie1, movie2, movie3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("action");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Title == "The Matrix");
            result.Should().Contain(m => m.Title == "Avatar");
            result.Should().NotContain(m => m.Title == "Titanic");
        }

        [Test]
        public async Task SearchAsync_WithDirectorMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            movie1.UpdateDetails(director: "Lana Wachowski, Lilly Wachowski");
            var movie2 = new Movie("Inception", "2010", "movie");
            movie2.UpdateDetails(director: "Christopher Nolan");

            _context.Movies.AddRange(movie1, movie2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("wachowski");

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(m => m.Title == "The Matrix");
            result.Should().NotContain(m => m.Title == "Inception");
        }

        [Test]
        public async Task SearchAsync_WithActorMatch_ShouldReturnMatchingMovies()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            movie1.UpdateDetails(actors: "Keanu Reeves, Laurence Fishburne");
            var movie2 = new Movie("John Wick", "2014", "movie");
            movie2.UpdateDetails(actors: "Keanu Reeves, Michael Nyqvist");

            _context.Movies.AddRange(movie1, movie2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("keanu");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Title == "The Matrix");
            result.Should().Contain(m => m.Title == "John Wick");
        }

        [Test]
        public async Task SearchAsync_WithEmptyQuery_ShouldReturnAllMoviesWithPrices()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            var movie2 = new Movie("Avatar", "2009", "movie");
            _context.Movies.AddRange(movie1, movie2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("");

            // Assert
            result.Should().HaveCount(2);
        }

        [Test]
        public async Task SearchAsync_WithWhitespaceQuery_ShouldReturnAllMoviesWithPrices()
        {
            // Arrange
            var movie1 = new Movie("The Matrix", "1999", "movie");
            var movie2 = new Movie("Avatar", "2009", "movie");
            _context.Movies.AddRange(movie1, movie2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("   ");

            // Assert
            result.Should().HaveCount(2);
        }

        [Test]
        public async Task SearchAsync_WithNoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("nonexistent");

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task AddAsync_WithValidMovie_ShouldAddMovieToDatabase()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");

            // Act
            var result = await _repository.AddAsync(movie);

            // Assert
            result.Should().Be(movie);
            result.Id.Should().BeGreaterThan(0);

            var savedMovie = await _context.Movies.FindAsync(result.Id);
            savedMovie.Should().NotBeNull();
            savedMovie!.Title.Should().Be("The Matrix");
        }

        [Test]
        public async Task AddAsync_WithNullMovie_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = async () => await _repository.AddAsync(null!);
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("movie");
        }

        [Test]
        public async Task UpdateAsync_WithExistingMovie_ShouldUpdateMovie()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Modify the movie
            movie.UpdateDetails(genre: "Action, Sci-Fi");

            // Act
            await _repository.UpdateAsync(movie);

            // Assert
            var updatedMovie = await _context.Movies.FindAsync(movie.Id);
            updatedMovie.Should().NotBeNull();
            updatedMovie!.Genre.Should().Be("Action, Sci-Fi");
        }

        [Test]
        public async Task UpdateAsync_WithNullMovie_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = async () => await _repository.UpdateAsync(null!);
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("movie");
        }

        [Test]
        public async Task DeleteAsync_WithExistingMovie_ShouldRemoveMovieFromDatabase()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            var movieId = movie.Id;

            // Act
            await _repository.DeleteAsync(movieId);

            // Assert
            var deletedMovie = await _context.Movies.FindAsync(movieId);
            deletedMovie.Should().BeNull();
        }

        [Test]
        public async Task DeleteAsync_WithNonExistentMovie_ShouldNotThrow()
        {
            // Act & Assert
            var action = async () => await _repository.DeleteAsync(999);
            await action.Should().NotThrowAsync();
        }

        [Test]
        public async Task ExistsAsync_WithExistingMovie_ShouldReturnTrue()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(movie.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ExistsAsync_WithNonExistentMovie_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            _context.Movies.Add(movie);

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            movie.Id.Should().BeGreaterThan(0);
            var savedMovie = await _context.Movies.FindAsync(movie.Id);
            savedMovie.Should().NotBeNull();
        }

        [Test]
        public async Task SearchAsync_ShouldIncludePrices()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            var price = new MoviePrice(movie.Id, "Cinemaworld", 15.99m);
            movie.AddPrice(price);

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("matrix");

            // Assert
            result.Should().HaveCount(1);
            var foundMovie = result.First();
            foundMovie.MoviePrices.Should().HaveCount(1);
            foundMovie.MoviePrices.First().Provider.Should().Be("Cinemaworld");
        }

        [Test]
        public async Task SearchAsync_ShouldBeCaseInsensitive()
        {
            // Arrange
            var movie = new Movie("The Matrix", "1999", "movie");
            movie.UpdateDetails(genre: "Action, Sci-Fi", director: "Wachowski Sisters", actors: "Keanu Reeves");
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act & Assert
            var titleResult = await _repository.SearchAsync("MATRIX");
            titleResult.Should().HaveCount(1);

            var genreResult = await _repository.SearchAsync("ACTION");
            genreResult.Should().HaveCount(1);

            var directorResult = await _repository.SearchAsync("WACHOWSKI");
            directorResult.Should().HaveCount(1);

            var actorResult = await _repository.SearchAsync("KEANU");
            actorResult.Should().HaveCount(1);
        }
    }
}
