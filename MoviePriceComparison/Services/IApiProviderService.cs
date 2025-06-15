using MoviePriceComparison.Models;

namespace MoviePriceComparison.Services
{
    public interface IApiProviderService
    {
        Task<List<ApiProvider>> GetApiProvidersAsync();
        Task<ApiProvider?> GetApiProviderAsync(string providerId);
        Task RefreshApiProvidersAsync();
        Task<bool> IsProviderEnabledAsync(string providerId);
    }
}
