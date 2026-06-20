using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int customerId);
    }
}
