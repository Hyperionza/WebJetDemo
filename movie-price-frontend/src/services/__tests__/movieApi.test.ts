import { ApiHealth, MovieComparison, MovieDetail } from '../../types/Movie';
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
                    id: 1,
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
                id: 1,
                title: 'The Matrix',
                year: '1999',
                genre: 'Action, Sci-Fi',
                director: 'Wachowski Sisters',
                plot: 'A computer programmer discovers reality is a simulation.',
                prices: []
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
                id: 42,
                title: 'Test Movie',
                prices: []
            };

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockMovieDetail,
            } as Response);

            await movieApi.getMovieDetail(42);

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/42');
        });
    });

    describe('searchMovies', () => {
        test('should search movies successfully', async () => {
            const mockSearchResults: MovieComparison[] = [
                {
                    id: 1,
                    title: 'The Matrix',
                    year: '1999',
                    prices: []
                }
            ];

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockSearchResults,
            } as Response);

            const result = await movieApi.searchMovies('matrix');

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/search?query=matrix');
            expect(result).toEqual(mockSearchResults);
        });

        test('should encode search query properly', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => [],
            } as Response);

            await movieApi.searchMovies('the matrix & reloaded');

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/search?query=the%20matrix%20%26%20reloaded');
        });

        test('should handle empty search results', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => [],
            } as Response);

            const result = await movieApi.searchMovies('nonexistent');

            expect(result).toEqual([]);
        });

        test('should throw error when search fails', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status: 400,
                statusText: 'Bad Request',
            } as Response);

            await expect(movieApi.searchMovies('invalid')).rejects.toThrow('Failed to search movies');
        });

        test('should handle special characters in search query', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => [],
            } as Response);

            await movieApi.searchMovies('movie with "quotes" and symbols!@#$%');

            expect(mockFetch).toHaveBeenCalledWith(
                expect.stringContaining('movie%20with%20%22quotes%22%20and%20symbols!%40%23%24%25')
            );
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

    describe('getApiHealth', () => {
        test('should fetch API health successfully', async () => {
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

            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => mockApiHealth,
            } as Response);

            const result = await movieApi.getApiHealth();

            expect(mockFetch).toHaveBeenCalledWith('http://localhost:5091/api/movies/health');
            expect(result).toEqual(mockApiHealth);
        });

        test('should throw error when health check fails', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: false,
                status: 503,
                statusText: 'Service Unavailable',
            } as Response);

            await expect(movieApi.getApiHealth()).rejects.toThrow('Failed to fetch API health');
        });

        test('should handle empty health response', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => [],
            } as Response);

            const result = await movieApi.getApiHealth();

            expect(result).toEqual([]);
        });
    });

    describe('error handling', () => {
        test('should handle JSON parsing errors', async () => {
            mockFetch.mockResolvedValueOnce({
                ok: true,
                json: async () => {
                    throw new Error('Invalid JSON');
                },
            } as Response);

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
            await movieApi.searchMovies('test');
            await movieApi.getApiHealth();
            await movieApi.refreshMovieData();

            const calls = mockFetch.mock.calls;
            expect(calls[0][0]).toBe(`${baseUrl}/movies`);
            expect(calls[1][0]).toBe(`${baseUrl}/movies/1`);
            expect(calls[2][0]).toBe(`${baseUrl}/movies/search?query=test`);
            expect(calls[3][0]).toBe(`${baseUrl}/movies/health`);
            expect(calls[4][0]).toBe(`${baseUrl}/movies/refresh`);
        });

        test('should use correct HTTP methods', async () => {
            mockFetch.mockResolvedValue({
                ok: true,
                json: async () => ({}),
            } as Response);

            await movieApi.getMovies();
            await movieApi.getMovieDetail(1);
            await movieApi.searchMovies('test');
            await movieApi.getApiHealth();
            await movieApi.refreshMovieData();

            const calls = mockFetch.mock.calls;

            // GET requests (default method)
            expect(calls[0][1]).toBeUndefined(); // getMovies
            expect(calls[1][1]).toBeUndefined(); // getMovieDetail
            expect(calls[2][1]).toBeUndefined(); // searchMovies
            expect(calls[3][1]).toBeUndefined(); // getApiHealth

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
