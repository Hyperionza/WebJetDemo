using Microsoft.AspNetCore.Mvc;
using MoviePriceComparison.Models;

namespace MoviePriceComparison.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MockConfigurationController : ControllerBase
    {
        private readonly ILogger<MockConfigurationController> _logger;

        public MockConfigurationController(ILogger<MockConfigurationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Mock configuration service endpoint that returns API provider configurations
        /// </summary>
        /// <returns>List of API providers with their configurations</returns>
        [HttpGet("api-providers")]
        public ActionResult<ApiProvidersResponse> GetApiProviders()
        {
            _logger.LogInformation("Mock configuration service: Returning API providers configuration");

            var response = new ApiProvidersResponse
            {
                Providers = new List<ApiProvider>
                {
                    new ApiProvider
                    {
                        Id = "cinemaworld",
                        Name = "cinemaworld",
                        DisplayName = "Cinemaworld",
                        BaseUrl = "https://webjetapitest.azurewebsites.net/api/cinemaworld",
                        ApiToken = Environment.GetEnvironmentVariable("CINEMAWORLD_API_TOKEN") ?? "demo-token-cinemaworld",
                        IsEnabled = true,
                        Priority = 1,
                        TimeoutSeconds = 30,
                        Headers = new Dictionary<string, string>
                        {
                            { "x-access-token", Environment.GetEnvironmentVariable("CINEMAWORLD_API_TOKEN") ?? "demo-token-cinemaworld" },
                            { "Content-Type", "application/json" },
                            { "User-Agent", "MoviePriceComparison/1.0" }
                        },
                        Endpoints = new ApiEndpoints
                        {
                            Movies = "/movies",
                            MovieDetail = "/movie/{id}",
                            Health = "/health"
                        },
                        LastUpdated = DateTime.UtcNow
                    },
                    new ApiProvider
                    {
                        Id = "filmworld",
                        Name = "filmworld",
                        DisplayName = "Filmworld",
                        BaseUrl = "https://webjetapitest.azurewebsites.net/api/filmworld",
                        ApiToken = Environment.GetEnvironmentVariable("FILMWORLD_API_TOKEN") ?? "demo-token-filmworld",
                        IsEnabled = true,
                        Priority = 2,
                        TimeoutSeconds = 30,
                        Headers = new Dictionary<string, string>
                        {
                            { "x-access-token", Environment.GetEnvironmentVariable("FILMWORLD_API_TOKEN") ?? "demo-token-filmworld" },
                            { "Content-Type", "application/json" },
                            { "User-Agent", "MoviePriceComparison/1.0" }
                        },
                        Endpoints = new ApiEndpoints
                        {
                            Movies = "/movies",
                            MovieDetail = "/movie/{id}",
                            Health = "/health"
                        },
                        LastUpdated = DateTime.UtcNow
                    },
                    // Example of a disabled provider
                    new ApiProvider
                    {
                        Id = "movieworld",
                        Name = "movieworld",
                        DisplayName = "MovieWorld",
                        BaseUrl = "https://api.movieworld.example.com",
                        ApiToken = "disabled-provider-token",
                        IsEnabled = false, // This provider is disabled
                        Priority = 3,
                        TimeoutSeconds = 20,
                        Headers = new Dictionary<string, string>
                        {
                            { "Authorization", "Bearer disabled-provider-token" },
                            { "Content-Type", "application/json" }
                        },
                        Endpoints = new ApiEndpoints
                        {
                            Movies = "/api/v1/movies",
                            MovieDetail = "/api/v1/movies/{id}",
                            Health = "/api/v1/health"
                        },
                        LastUpdated = DateTime.UtcNow.AddDays(-1)
                    }
                },
                LastUpdated = DateTime.UtcNow,
                Version = "1.0"
            };

            return Ok(response);
        }

        /// <summary>
        /// Mock endpoint to simulate updating provider configurations
        /// </summary>
        /// <param name="providerId">Provider ID to update</param>
        /// <param name="isEnabled">Whether to enable or disable the provider</param>
        /// <returns>Updated provider configuration</returns>
        [HttpPatch("api-providers/{providerId}/status")]
        public ActionResult<ApiProvider> UpdateProviderStatus(string providerId, [FromBody] bool isEnabled)
        {
            _logger.LogInformation("Mock configuration service: Updating provider {ProviderId} status to {IsEnabled}",
                providerId, isEnabled);

            // In a real implementation, this would update the provider in a database
            // For this mock, we'll just return a sample updated provider
            var updatedProvider = new ApiProvider
            {
                Id = providerId,
                Name = providerId,
                DisplayName = providerId.Substring(0, 1).ToUpper() + providerId.Substring(1),
                IsEnabled = isEnabled,
                LastUpdated = DateTime.UtcNow
            };

            return Ok(updatedProvider);
        }

        /// <summary>
        /// Mock endpoint to add a new API provider
        /// </summary>
        /// <param name="provider">New provider configuration</param>
        /// <returns>Created provider</returns>
        [HttpPost("api-providers")]
        public ActionResult<ApiProvider> AddApiProvider([FromBody] ApiProvider provider)
        {
            _logger.LogInformation("Mock configuration service: Adding new provider {ProviderId}", provider.Id);

            // Validate the provider
            if (string.IsNullOrEmpty(provider.Id) || string.IsNullOrEmpty(provider.BaseUrl))
            {
                return BadRequest("Provider ID and BaseUrl are required");
            }

            // Set default values
            provider.LastUpdated = DateTime.UtcNow;
            if (provider.TimeoutSeconds <= 0)
                provider.TimeoutSeconds = 30;

            return CreatedAtAction(nameof(GetApiProviders), new { id = provider.Id }, provider);
        }

        /// <summary>
        /// Mock endpoint to simulate configuration service health
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        public ActionResult<object> GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "Mock Configuration Service",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                ProvidersCount = 3
            });
        }
    }
}
