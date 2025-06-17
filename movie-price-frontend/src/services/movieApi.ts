import { MovieComparison, MovieDetail } from '../types/Movie';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://127.0.0.1:5091';

class MovieApiService {
  async getMovies(): Promise<MovieComparison[]> {
    const response = await fetch(`${API_BASE_URL}/api/movies`);
    if (!response.ok) {
      throw new Error('Failed to fetch movies');
    }
    return response.json();
  }

  async getMovieDetail(id: number): Promise<MovieDetail> {
    const response = await fetch(`${API_BASE_URL}/api/movies/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch movie detail');
    }
    return response.json();
  }

  async refreshMovieData(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/movies/refresh`, {
      method: 'POST',
    });
    if (!response.ok) {
      throw new Error('Failed to refresh movie data');
    }
  }
}

export const movieApi = new MovieApiService();
