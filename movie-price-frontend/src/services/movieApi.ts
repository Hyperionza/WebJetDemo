import { MovieComparison, MovieDetail, ApiHealth } from '../types/Movie';

const API_BASE_URL = 'http://localhost:5091/api';

class MovieApiService {
  async getMovies(): Promise<MovieComparison[]> {
    const response = await fetch(`${API_BASE_URL}/movies`);
    if (!response.ok) {
      throw new Error('Failed to fetch movies');
    }
    return response.json();
  }

  async getMovieDetail(id: number): Promise<MovieDetail> {
    const response = await fetch(`${API_BASE_URL}/movies/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch movie detail');
    }
    return response.json();
  }

  async searchMovies(query: string): Promise<MovieComparison[]> {
    const response = await fetch(`${API_BASE_URL}/movies/search?query=${encodeURIComponent(query)}`);
    if (!response.ok) {
      throw new Error('Failed to search movies');
    }
    return response.json();
  }

  async refreshMovieData(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/movies/refresh`, {
      method: 'POST',
    });
    if (!response.ok) {
      throw new Error('Failed to refresh movie data');
    }
  }

  async getApiHealth(): Promise<ApiHealth[]> {
    const response = await fetch(`${API_BASE_URL}/movies/health`);
    if (!response.ok) {
      throw new Error('Failed to fetch API health');
    }
    return response.json();
  }
}

export const movieApi = new MovieApiService();
