using MoviePriceComparison.Application.DTOs;

namespace MoviePriceComparison.Domain.Services
{
    public interface IApiProviderService
    {
        Task<List<ApiProvider>> GetApiProvidersAsync();
        Task<ApiProvider?> GetApiProviderAsync(string providerId);
        Task RefreshApiProvidersAsync();
        Task<bool> IsProviderEnabledAsync(string providerId);
    }
}
