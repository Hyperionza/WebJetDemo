import '@testing-library/jest-dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import App from '../App';
import { movieApi } from '../services/movieApi';
import { MovieComparison } from '../types/Movie';

// Mock the movieApi service
jest.mock('../services/movieApi');
const mockMovieApi = movieApi as jest.Mocked<typeof movieApi>;

// Mock data
const mockMovies: MovieComparison[] = [
    {
        id: '1',
        title: 'The Matrix',
        year: '1999',
        genre: 'Action, Sci-Fi',
        director: 'Wachowski Sisters',
        poster: 'https://example.com/matrix.jpg',
        rating: '8.7',
        cheapestPrice: {
            providerId: 'filmworld',
            provider: 'Filmworld',
            movieId: 'fw001',
            price: 14.99,
            lastUpdated: '2023-01-01T00:00:00Z'
        },
        prices: [
            {
                providerId: 'cinemaworld',
                provider: 'Cinemaworld',
                movieId: 'cw001',
                price: 15.99,
                lastUpdated: '2023-01-01T00:00:00Z'
            },
            {
                providerId: 'filmworld',
                provider: 'Filmworld',
                movieId: 'fw001',
                price: 14.99,
                lastUpdated: '2023-01-01T00:00:00Z'
            }
        ]
    },
    {
        id: '2',
        title: 'Inception',
        year: '2010',
        genre: 'Action, Thriller',
        director: 'Christopher Nolan',
        rating: '8.8',
        prices: [
            {
                providerId: 'cinemaworld',
                provider: 'Cinemaworld',
                movieId: 'cw002',
                price: 18.99,
                lastUpdated: '2023-01-01T00:00:00Z'
            }
        ]
    }
];

describe('App', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockMovieApi.getMovies.mockResolvedValue(mockMovies);
        mockMovieApi.refreshMovieData.mockResolvedValue();
    });

    test('renders app header and title', async () => {
        render(<App />);

        expect(screen.getByText('🎬 Movie Price Comparison')).toBeInTheDocument();

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
        });

        await waitFor(() => {
            expect(screen.getAllByText('Inception')).toHaveLength(2); // Placeholder + title
        });

        expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(1);
    });

    test('displays movie count', async () => {
        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('Found 2 movies')).toBeInTheDocument();
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
        });

        await waitFor(() => {
            expect(mockMovieApi.getMovies).toHaveBeenCalledTimes(2); // Initial + after refresh
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

        // Click on a movie card - use getByRole to find the clickable element
        const movieCard = screen.getByRole('button', { name: /the matrix/i });
        fireEvent.click(movieCard);

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
        });

        await waitFor(() => {
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
        });

        await waitFor(() => {
            expect(screen.queryByText('Error: Network error')).not.toBeInTheDocument();
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
        });

        await waitFor(() => {
            expect(screen.getByText('No movies found. Try refreshing the data or check your search query.')).toBeInTheDocument();
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
