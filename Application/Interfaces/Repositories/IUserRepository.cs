using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailWithRolesAsync(string email);
        Task<User?> GetByIdWithRolesAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
    }
}
