using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
    }
}
