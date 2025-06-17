import { render, screen } from '@testing-library/react';
import App from './App';
import { movieApi } from './services/movieApi';

// Mock the movieApi service
jest.mock('./services/movieApi');
const mockMovieApi = movieApi as jest.Mocked<typeof movieApi>;

test('renders movie price comparison app', async () => {
  // Mock API responses
  mockMovieApi.getMovies.mockResolvedValue([]);

  render(<App />);

  // Check for main heading
  expect(screen.getByText('🎬 Movie Price Comparison')).toBeInTheDocument();
});
