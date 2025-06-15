import '@testing-library/jest-dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import App from '../App';
import { movieApi } from '../services/movieApi';
import { ApiHealth, MovieComparison } from '../types/Movie';

// Mock the movieApi service
jest.mock('../services/movieApi');
const mockMovieApi = movieApi as jest.Mocked<typeof movieApi>;

// Mock data
const mockMovies: MovieComparison[] = [
    {
        id: 1,
        title: 'The Matrix',
        year: '1999',
        genre: 'Action, Sci-Fi',
        director: 'Wachowski Sisters',
        poster: 'https://example.com/matrix.jpg',
        rating: '8.7',
        bestPrice: {
            provider: 'Filmworld',
            price: 14.99,
            freshness: 'Fresh',
            lastUpdated: '2023-01-01T00:00:00Z',
            freshnessIndicator: '🟢'
        },
        prices: [
            {
                provider: 'Cinemaworld',
                price: 15.99,
                freshness: 'Fresh',
                lastUpdated: '2023-01-01T00:00:00Z',
                freshnessIndicator: '🟢'
            },
            {
                provider: 'Filmworld',
                price: 14.99,
                freshness: 'Fresh',
                lastUpdated: '2023-01-01T00:00:00Z',
                freshnessIndicator: '🟢'
            }
        ]
    },
    {
        id: 2,
        title: 'Inception',
        year: '2010',
        genre: 'Action, Thriller',
        director: 'Christopher Nolan',
        rating: '8.8',
        prices: [
            {
                provider: 'Cinemaworld',
                price: 18.99,
                freshness: 'Cached',
                lastUpdated: '2023-01-01T00:00:00Z',
                freshnessIndicator: '🟡'
            }
        ]
    }
];

const mockApiHealth: ApiHealth[] = [
    {
        provider: 'Cinemaworld',
        isHealthy: true,
        lastChecked: '2023-01-01T00:00:00Z'
    },
    {
        provider: 'Filmworld',
        isHealthy: false,
        lastChecked: '2023-01-01T00:00:00Z',
        errorMessage: 'Service unavailable'
    }
];

describe('App', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockMovieApi.getMovies.mockResolvedValue(mockMovies);
        mockMovieApi.getApiHealth.mockResolvedValue(mockApiHealth);
        mockMovieApi.searchMovies.mockResolvedValue([]);
        mockMovieApi.refreshMovieData.mockResolvedValue();
    });

    test('renders app header and title', async () => {
        render(<App />);

        expect(screen.getByText('🎬 Movie Price Comparison')).toBeInTheDocument();
        expect(screen.getByText('Compare movie prices from Cinemaworld and Filmworld')).toBeInTheDocument();

        // Wait for loading to complete
        await waitFor(() => {
            expect(screen.queryByText('Loading movies...')).not.toBeInTheDocument();
        });
    });

    test('loads and displays movies on initial render', async () => {
        render(<App />);

        // Should show loading initially
        expect(screen.getByText('Loading movies...')).toBeInTheDocument();

        // Wait for movies to load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
            expect(screen.getAllByText('Inception')).toHaveLength(2); // Placeholder + title
        });

        expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(1);
        expect(mockMovieApi.getApiHealth).toHaveBeenCalledTimes(1);
    });

    test('displays API health status', async () => {
        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('Cinemaworld: ✅')).toBeInTheDocument();
            expect(screen.getByText('Filmworld: ❌')).toBeInTheDocument();
        });
    });

    test('displays movie count', async () => {
        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('Found 2 movies')).toBeInTheDocument();
        });
    });

    test('handles search functionality', async () => {
        const searchResults = [mockMovies[0]]; // Only The Matrix
        mockMovieApi.searchMovies.mockResolvedValue(searchResults);

        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Perform search
        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchButton = screen.getByText('Search');

        fireEvent.change(searchInput, { target: { value: 'matrix' } });
        fireEvent.click(searchButton);

        await waitFor(() => {
            expect(mockMovieApi.searchMovies).toHaveBeenCalledWith('matrix');
            expect(screen.getByText('Found 1 movie')).toBeInTheDocument();
        });
    });

    test('handles empty search query by loading all movies', async () => {
        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Clear search and submit
        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchButton = screen.getByText('Search');

        fireEvent.change(searchInput, { target: { value: '' } });
        fireEvent.click(searchButton);

        await waitFor(() => {
            expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(2); // Initial + after empty search
        });
    });

    test('handles search form submission', async () => {
        mockMovieApi.searchMovies.mockResolvedValue([mockMovies[0]]);

        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Submit search form
        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchForm = searchInput.closest('form');

        fireEvent.change(searchInput, { target: { value: 'matrix' } });
        fireEvent.submit(searchForm!);

        await waitFor(() => {
            expect(mockMovieApi.searchMovies).toHaveBeenCalledWith('matrix');
        });
    });

    test('handles refresh functionality', async () => {
        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Click refresh button
        const refreshButton = screen.getByText('🔄 Refresh Data');
        fireEvent.click(refreshButton);

        // Should show refreshing state
        expect(screen.getByText('🔄 Refreshing...')).toBeInTheDocument();

        await waitFor(() => {
            expect(mockMovieApi.refreshMovieData).toHaveBeenCalledTimes(1);
            expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(2); // Initial + after refresh
            expect(mockMovieApi.getApiHealth).toHaveBeenCalledTimes(2); // Initial + after refresh
        });

        // Should return to normal state
        await waitFor(() => {
            expect(screen.getByText('🔄 Refresh Data')).toBeInTheDocument();
        });
    });

    test('disables refresh button while refreshing', async () => {
        // Make refresh take longer to test disabled state
        mockMovieApi.refreshMovieData.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        const refreshButton = screen.getByText('🔄 Refresh Data');
        fireEvent.click(refreshButton);

        // Button should be disabled while refreshing
        expect(screen.getByText('🔄 Refreshing...')).toBeDisabled();
    });

    test('handles movie click to show details', async () => {
        // Mock window.alert
        const alertSpy = jest.spyOn(window, 'alert').mockImplementation(() => { });

        render(<App />);

        // Wait for movies to load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Click on a movie card
        const movieCard = screen.getByText('The Matrix').closest('.movie-card');
        fireEvent.click(movieCard!);

        expect(alertSpy).toHaveBeenCalledWith(
            expect.stringContaining('Movie Details:')
        );
        expect(alertSpy).toHaveBeenCalledWith(
            expect.stringContaining('The Matrix')
        );

        alertSpy.mockRestore();
    });

    test('displays error message when API fails', async () => {
        const errorMessage = 'Failed to load movies';
        mockMovieApi.getMovies.mockRejectedValue(new Error(errorMessage));

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText(`Error: ${errorMessage}`)).toBeInTheDocument();
            expect(screen.getByText('Try Again')).toBeInTheDocument();
        });
    });

    test('handles try again button after error', async () => {
        mockMovieApi.getMovies.mockRejectedValueOnce(new Error('Network error'));

        render(<App />);

        // Wait for error to appear
        await waitFor(() => {
            expect(screen.getByText('Error: Network error')).toBeInTheDocument();
        });

        // Mock successful retry
        mockMovieApi.getMovies.mockResolvedValue(mockMovies);

        // Click try again
        const tryAgainButton = screen.getByText('Try Again');
        fireEvent.click(tryAgainButton);

        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
            expect(screen.queryByText('Error: Network error')).not.toBeInTheDocument();
        });
    });

    test('handles search error', async () => {
        mockMovieApi.searchMovies.mockRejectedValue(new Error('Search failed'));

        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Perform search that fails
        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchButton = screen.getByText('Search');

        fireEvent.change(searchInput, { target: { value: 'test' } });
        fireEvent.click(searchButton);

        await waitFor(() => {
            expect(screen.getByText('Error: Search failed')).toBeInTheDocument();
        });
    });

    test('handles refresh error', async () => {
        mockMovieApi.refreshMovieData.mockRejectedValue(new Error('Refresh failed'));

        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Click refresh button
        const refreshButton = screen.getByText('🔄 Refresh Data');
        fireEvent.click(refreshButton);

        await waitFor(() => {
            expect(screen.getByText('Error: Refresh failed')).toBeInTheDocument();
        });
    });

    test('displays no movies message when list is empty', async () => {
        mockMovieApi.getMovies.mockResolvedValue([]);

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('Found 0 movies')).toBeInTheDocument();
            expect(screen.getByText('No movies found. Try refreshing the data or check your search query.')).toBeInTheDocument();
        });
    });

    test('displays footer with freshness indicators', async () => {
        render(<App />);

        expect(screen.getByText('Data freshness indicators: 🟢 Fresh | 🟡 Cached | 🔴 Stale')).toBeInTheDocument();
    });

    test('handles API health loading failure gracefully', async () => {
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => { });
        mockMovieApi.getApiHealth.mockRejectedValue(new Error('Health check failed'));

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        // Should not crash the app, just log the error
        expect(consoleSpy).toHaveBeenCalledWith('Failed to load API health:', expect.any(Error));

        consoleSpy.mockRestore();
    });

    test('updates search input value correctly', async () => {
        render(<App />);

        const searchInput = screen.getByPlaceholderText('Search movies...') as HTMLInputElement;

        fireEvent.change(searchInput, { target: { value: 'test query' } });

        expect(searchInput.value).toBe('test query');
    });

    test('clears search input and loads all movies when search is cleared', async () => {
        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchButton = screen.getByText('Search');

        // Type and clear search
        fireEvent.change(searchInput, { target: { value: 'test' } });
        fireEvent.change(searchInput, { target: { value: '' } });
        fireEvent.click(searchButton);

        await waitFor(() => {
            expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(2); // Initial + after clearing search
        });
    });

    test('handles whitespace-only search query', async () => {
        render(<App />);

        // Wait for initial load
        await waitFor(() => {
            expect(screen.getByText('The Matrix')).toBeInTheDocument();
        });

        const searchInput = screen.getByPlaceholderText('Search movies...');
        const searchButton = screen.getByText('Search');

        fireEvent.change(searchInput, { target: { value: '   ' } });
        fireEvent.click(searchButton);

        await waitFor(() => {
            expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(2); // Initial + after whitespace search
        });
    });

    test('displays correct singular/plural movie count', async () => {
        // Test with single movie
        mockMovieApi.getMovies.mockResolvedValue([mockMovies[0]]);

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('Found 1 movie')).toBeInTheDocument();
        });
    });
});
