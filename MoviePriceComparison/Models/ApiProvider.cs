namespace MoviePriceComparison.Models
{
    public class ApiProvider
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 1;
        public int TimeoutSeconds { get; set; } = 30;
        public Dictionary<string, string> Headers { get; set; } = new();
        public ApiEndpoints Endpoints { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class ApiEndpoints
    {
        public string Movies { get; set; } = "/movies";
        public string MovieDetail { get; set; } = "/movie/{id}";
        public string Health { get; set; } = "/health";
    }

    public class ApiProvidersResponse
    {
        public List<ApiProvider> Providers { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
    }
}
