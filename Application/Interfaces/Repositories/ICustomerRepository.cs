using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByIdWithOrders(int id);
        Task<Customer?> GetByIdWithCart(int id);
        Task<Customer?> GetByIdWithReviews(int id);
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<Customer?> GetByUserIdWithDetailsAsync(int userId);
    }
}
