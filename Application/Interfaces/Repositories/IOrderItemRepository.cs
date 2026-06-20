using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
    }
}
