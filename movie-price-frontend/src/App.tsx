import React, { useState, useEffect } from 'react';
import { MovieComparison, ApiHealth } from './types/Movie';
import { movieApi } from './services/movieApi';
import MovieCard from './components/MovieCard';
import './App.css';

function App() {
  const [movies, setMovies] = useState<MovieComparison[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [apiHealth, setApiHealth] = useState<ApiHealth[]>([]);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    loadMovies();
    loadApiHealth();
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

  const loadApiHealth = async () => {
    try {
      const healthData = await movieApi.getApiHealth();
      setApiHealth(healthData);
    } catch (err) {
      console.error('Failed to load API health:', err);
    }
  };

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchQuery.trim()) {
      loadMovies();
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const searchResults = await movieApi.searchMovies(searchQuery);
      setMovies(searchResults);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to search movies');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    try {
      setRefreshing(true);
      await movieApi.refreshMovieData();
      await loadMovies();
      await loadApiHealth();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh data');
    } finally {
      setRefreshing(false);
    }
  };

  const handleMovieClick = (movie: MovieComparison) => {
    alert(`Movie Details:\n\nTitle: ${movie.title}\nYear: ${movie.year}\nGenre: ${movie.genre}\nDirector: ${movie.director}\nRating: ${movie.rating}\n\nPrices:\n${movie.prices.map(p => `${p.provider}: $${p.price.toFixed(2)} ${p.freshnessIndicator}`).join('\n')}`);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>🎬 Movie Price Comparison</h1>
        <p>Compare movie prices from Cinemaworld and Filmworld</p>
        
        {/* API Health Status */}
        <div className="api-health">
          {apiHealth.map((health, index) => (
            <div key={index} className={`health-indicator ${health.isHealthy ? 'healthy' : 'unhealthy'}`}>
              {health.provider}: {health.isHealthy ? '✅' : '❌'}
            </div>
          ))}
        </div>

        {/* Search and Refresh */}
        <div className="controls">
          <form onSubmit={handleSearch} className="search-form">
            <input
              type="text"
              placeholder="Search movies..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="search-input"
            />
            <button type="submit" className="search-button">Search</button>
          </form>
          
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

      <footer className="App-footer">
        <p>Data freshness indicators: 🟢 Fresh | 🟡 Cached | 🔴 Stale</p>
      </footer>
    </div>
  );
}

export default App;
