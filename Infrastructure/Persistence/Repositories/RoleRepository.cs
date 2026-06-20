using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<Role?> GetByNameAsync(string roleName) =>
            await _context.Roles
                .SingleOrDefaultAsync(r => r.RoleName == roleName);
    }
}
