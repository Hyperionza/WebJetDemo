import '@testing-library/jest-dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MovieComparison } from '../../types/Movie';
import MovieCard from '../MovieCard';

// Mock movie data
const mockMovie: MovieComparison = {
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
};

const mockMovieWithoutPoster: MovieComparison = {
    ...mockMovie,
    poster: undefined
};

const mockMovieWithoutBestPrice: MovieComparison = {
    ...mockMovie,
    cheapestPrice: undefined
};

describe('MovieCard', () => {
    const mockOnClick = jest.fn();

    beforeEach(() => {
        mockOnClick.mockClear();
    });

    test('renders movie information correctly', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        expect(screen.getByText('The Matrix')).toBeInTheDocument();
        expect(screen.getByText('1999')).toBeInTheDocument();
        expect(screen.getByText('Action, Sci-Fi')).toBeInTheDocument();
        expect(screen.getByText('⭐ 8.7')).toBeInTheDocument();
    });

    test('displays best price when available', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        expect(screen.getByText('Best Price:')).toBeInTheDocument();
        expect(screen.getAllByText('$14.99')).toHaveLength(2); // Best price + provider price
    });

    test('does not display best price section when not available', () => {
        render(<MovieCard movie={mockMovieWithoutBestPrice} onClick={mockOnClick} />);

        expect(screen.queryByText('Best Price:')).not.toBeInTheDocument();
    });

    test('displays all provider prices', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        expect(screen.getByText('Cinemaworld:')).toBeInTheDocument();
        expect(screen.getByText('Filmworld:')).toBeInTheDocument();
        expect(screen.getByText('$15.99')).toBeInTheDocument();
        expect(screen.getAllByText('$14.99')).toHaveLength(2); // Best price + provider price
    });

    test('calls onClick when card is clicked', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        const card = screen.getByRole('button', { name: /the matrix/i });
        fireEvent.click(card);

        expect(mockOnClick).toHaveBeenCalledWith(mockMovie);
    });

    test('displays poster image when available', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        const image = screen.getByAltText('The Matrix');
        expect(image).toBeInTheDocument();
        expect(image).toHaveAttribute('src', 'https://example.com/matrix.jpg');
    });

    test('displays placeholder when poster is not available', () => {
        render(<MovieCard movie={mockMovieWithoutPoster} onClick={mockOnClick} />);

        expect(screen.getByText('🎬')).toBeInTheDocument();
        expect(screen.getAllByText('The Matrix')).toHaveLength(2);
    });

    test('handles image load error gracefully', async () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        const image = screen.getByAltText('The Matrix');
        fireEvent.error(image);

        await waitFor(() => {
            expect(screen.getByText('🎬')).toBeInTheDocument();
        });
    });

    test('shows loading state for image', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        // Initially, loading spinner should be present - use data-testid or other Testing Library queries
        const loadingElement = screen.queryByTestId('poster-loading');
        expect(loadingElement).toBeInTheDocument();
    });

    test('hides loading state when image loads', async () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        const image = screen.getByAltText('The Matrix');
        fireEvent.load(image);

        await waitFor(() => {
            const loadingElement = screen.queryByTestId('poster-loading');
            expect(loadingElement).not.toBeInTheDocument();
        });
    });

    test('formats prices correctly', () => {
        const movieWithDecimalPrice: MovieComparison = {
            ...mockMovie,
            prices: [
                {
                    providerId: 'testprovider',
                    provider: 'TestProvider',
                    movieId: 'test001',
                    price: 12.5,
                    lastUpdated: '2023-01-01T00:00:00Z'
                }
            ]
        };

        render(<MovieCard movie={movieWithDecimalPrice} onClick={mockOnClick} />);

        expect(screen.getByText('$12.50')).toBeInTheDocument();
    });

    test('handles empty prices array', () => {
        const movieWithNoPrices: MovieComparison = {
            ...mockMovie,
            prices: []
        };

        render(<MovieCard movie={movieWithNoPrices} onClick={mockOnClick} />);

        expect(screen.getByText('The Matrix')).toBeInTheDocument();
        // Should still render the card but without price information
    });

    test('has proper accessibility attributes', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        const image = screen.getByAltText('The Matrix');
        expect(image).toHaveAttribute('alt', 'The Matrix');
    });

    test('handles missing optional fields gracefully', () => {
        const minimalMovie: MovieComparison = {
            id: '1',
            title: 'Minimal Movie',
            prices: []
        };

        render(<MovieCard movie={minimalMovie} onClick={mockOnClick} />);

        expect(screen.getAllByText('Minimal Movie')).toHaveLength(2); // Placeholder + title
        expect(screen.getByText('🎬')).toBeInTheDocument(); // Should show placeholder
    });

    test('displays movie rating when available', () => {
        render(<MovieCard movie={mockMovie} onClick={mockOnClick} />);

        expect(screen.getByText('⭐ 8.7')).toBeInTheDocument();
    });

    test('handles missing rating gracefully', () => {
        const movieWithoutRating: MovieComparison = {
            ...mockMovie,
            rating: undefined
        };

        render(<MovieCard movie={movieWithoutRating} onClick={mockOnClick} />);

        expect(screen.getByText('The Matrix')).toBeInTheDocument();
        expect(screen.queryByText(/⭐/)).not.toBeInTheDocument();
    });
});
