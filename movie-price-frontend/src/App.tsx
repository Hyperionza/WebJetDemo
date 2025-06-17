import { useEffect, useState } from 'react';
import './App.css';
import MovieCard from './components/MovieCard';
import { movieApi } from './services/movieApi';
import { MovieComparison } from './types/Movie';

function App() {
  const [movies, setMovies] = useState<MovieComparison[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    loadMovies();
  }, []);

  const loadMovies = async () => {
    try {
      setLoading(true);
      setError(null);
      const moviesData = await movieApi.getMovies();
      setMovies(moviesData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load movies');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    try {
      setRefreshing(true);
      await movieApi.refreshMovieData();
      await loadMovies();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh data');
    } finally {
      setRefreshing(false);
    }
  };

  const handleMovieClick = (movie: MovieComparison) => {
    const priceText = movie.prices.map(p => `${p.provider}: $${p.price.toFixed(2)}}`).join('\n');
    const cheapestText = movie.cheapestPrice ? `\nBest Price: ${movie.cheapestPrice.provider} - $${movie.cheapestPrice.price.toFixed(2)}` : '';
    alert(`Movie Details:\n\nTitle: ${movie.title}\nYear: ${movie.year || 'N/A'}\nGenre: ${movie.genre || 'N/A'}\nDirector: ${movie.director || 'N/A'}\nRating: ${movie.rating || 'N/A'}\n\nPrices:\n${priceText}${cheapestText}`);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>🎬 Movie Price Comparison</h1>

        {/* Search and Refresh */}
        <div className="controls">
          <button
            onClick={handleRefresh}
            disabled={refreshing}
            className="refresh-button"
          >
            {refreshing ? '🔄 Refreshing...' : '🔄 Refresh Data'}
          </button>
        </div>
      </header>

      <main className="App-main">
        {loading && <div className="loading">Loading movies...</div>}

        {error && (
          <div className="error">
            <p>Error: {error}</p>
            <button onClick={loadMovies}>Try Again</button>
          </div>
        )}

        {!loading && !error && (
          <>
            <div className="movies-count">
              Found {movies.length} movie{movies.length !== 1 ? 's' : ''}
            </div>

            <div className="movies-grid">
              {movies.map((movie) => (
                <MovieCard
                  key={movie.id}
                  movie={movie}
                  onClick={handleMovieClick}
                />
              ))}
            </div>

            {movies.length === 0 && (
              <div className="no-movies">
                No movies found. Try refreshing the data or check your search query.
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default App;
