namespace MoviePriceComparison.Infrastructure.Services
{
    public class ApiProviderConfiguration
    {
        public string ApiProviderServiceUrl { get; set; } = string.Empty;
        public int CacheDurationMinutes { get; set; } = 15;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
