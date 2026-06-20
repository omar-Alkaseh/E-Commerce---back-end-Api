using Contracts.Responses;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync();
        Task<List<TopProductResponse>> GetTopProductsAsync(int count = 5);
    }
}
