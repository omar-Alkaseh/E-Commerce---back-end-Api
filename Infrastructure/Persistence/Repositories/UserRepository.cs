using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<bool> EmailExistsAsync(string email) =>
            await _context.Users
                .AnyAsync(u => u.Email == email);

        public async Task<IEnumerable<User>> GetAllWithRolesAsync() =>
            await _context.Users
                .Include(u => u.Roles)
                .ToListAsync();

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByEmailWithRolesAsync(string email) =>
            await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdWithRolesAsync(int userId) =>
            await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}
