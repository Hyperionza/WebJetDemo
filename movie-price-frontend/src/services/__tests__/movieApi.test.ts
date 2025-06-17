import { MovieComparison, MovieDetail } from '../../types/Movie';
import { movieApi } from '../movieApi';

// Mock fetch globally
global.fetch = jest.fn();
const mockFetch = fetch as jest.MockedFunction<typeof fetch>;

describe('MovieApiService', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    describe('getMovies', () => {
        test('should fetch movies successfully', async () => {
            const mockMovies: MovieComparison[] = [
                {
                    id: '1',
                    title: 'The Matrix',
                    year: '1999',
                    prices: []
                }
            ];

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockMovies,
            } as any);

            const result = await movieApi.getMovies();

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies');
            expect(result).toEqual(mockMovies);
        });

        test('should throw error when response is not ok', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status: 500,
                statusText: 'Internal Server Error',
            } as any);

            await expect(movieApi.getMovies()).rejects.toThrow('Failed to fetch movies');
        });

        test('should handle network errors', async () => {
            mockFetch.mockRejectedValueOnce(new Error('Network error'));

            await expect(movieApi.getMovies()).rejects.toThrow('Network error');
        });
    });

    describe('getMovieDetail', () => {
        test('should fetch movie detail successfully', async () => {
            const mockMovieDetail: MovieDetail = {
                title: 'The Matrix',
                year: '1999',
                genre: 'Action, Sci-Fi',
                director: 'Wachowski Sisters',
                plot: 'A computer programmer discovers reality is a simulation.',
                prices: [],
                updatedAt: ''
            };

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockMovieDetail,
            } as any);

            const result = await movieApi.getMovieDetail(1);

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/1');
            expect(result).toEqual(mockMovieDetail);
        });

        test('should throw error when movie not found', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status: 404,
                statusText: 'Not Found',
            } as any);

            await expect(movieApi.getMovieDetail(999)).rejects.toThrow('Failed to fetch movie detail');
        });

        test('should handle different movie IDs correctly', async () => {
            const mockMovieDetail: MovieDetail = {
                title: 'Test Movie',
                prices: [],
                updatedAt: ''
            };

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockMovieDetail,
            } as Response);

            await movieApi.getMovieDetail(42);

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/42');
        });
    });

    describe('refreshMovieData', () => {
        test('should refresh movie data successfully', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
            } as Response);

            await movieApi.refreshMovieData();

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/refresh', {
                method: 'POST',
            });
        });

        test('should throw error when refresh fails', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status: 500,
                statusText: 'Internal Server Error',
            } as Response);

            await expect(movieApi.refreshMovieData()).rejects.toThrow('Failed to refresh movie data');
        });

        test('should handle network errors during refresh', async () => {
            mockFetch.mockRejectedValueOnce(new Error('Network timeout'));

            await expect(movieApi.refreshMovieData()).rejects.toThrow('Network timeout');
        });
    });

    describe('error handling', () => {
        test('should handle JSON parsing errors', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => {
                    throw new Error('Invalid JSON');
                },
            } as unknown as Response);

            await expect(movieApi.getMovies()).rejects.toThrow('Invalid JSON');
        });

        test('should handle fetch rejection', async () => {
            mockFetch.mockRejectedValueOnce(new Error('Fetch failed'));

            await expect(movieApi.getMovies()).rejects.toThrow('Fetch failed');
        });

        test('should handle undefined response', async () => {
            mockFetch.mockResolvedValueOnce(undefined as any);

            await expect(movieApi.getMovies()).rejects.toThrow();
        });
    });

    describe('API endpoints', () => {
        test('should use correct base URL for all endpoints', async () => {
            const baseUrl = 'http://localhost:5091/api';

            // Mock all responses
            mockFetch.mockResolvedValue({
                ok: true,
                json: async () => ({}),
            } as Response);

            // Test all endpoints
            await movieApi.getMovies();
            await movieApi.getMovieDetail(1);
            await movieApi.refreshMovieData();

            const calls = mockFetch.mock.calls;
            expect(calls[0][0]).toBe(`${baseUrl}/movies`);
            expect(calls[1][0]).toBe(`${baseUrl}/movies/1`);
            expect(calls[4][0]).toBe(`${baseUrl}/movies/refresh`);
        });

        test('should use correct HTTP methods', async () => {
            mockFetch.mockResolvedValue({
                ok: true,
                json: async () => ({}),
            } as Response);

            await movieApi.getMovies();
            await movieApi.getMovieDetail(1);
            await movieApi.refreshMovieData();

            const calls = mockFetch.mock.calls;

            // GET requests (default method)
            expect(calls[0][1]).toBeUndefined(); // getMovies
            expect(calls[1][1]).toBeUndefined(); // getMovieDetail

            // POST request
            expect(calls[4][1]).toEqual({ method: 'POST' }); // refreshMovieData
        });
    });

    describe('response status codes', () => {
        test.each([
            [400, 'Bad Request'],
            [401, 'Unauthorized'],
            [403, 'Forbidden'],
            [404, 'Not Found'],
            [500, 'Internal Server Error'],
            [502, 'Bad Gateway'],
            [503, 'Service Unavailable'],
        ])('should handle %d status code', async (status, statusText) => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status,
                statusText,
            } as Response);

            await expect(movieApi.getMovies()).rejects.toThrow('Failed to fetch movies');
        });

        test.each([
            [200, 'OK'],
            [201, 'Created'],
            [202, 'Accepted'],
            [204, 'No Content'],
        ])('should handle successful %d status code', async (status, statusText) => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                status,
                statusText,
                json: async () => [],
            } as Response);

            const result = await movieApi.getMovies();
            expect(result).toEqual([]);
        });
    });
});
