using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId) =>
            await _context.RefreshTokens
                .Where(
                    r => r.UserId == userId &&
                    r.RevokedAt == null &&
                    r.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
            await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(c => c.Roles)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }
}
