using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdWithDetailsAsync(int orderId);
        Task<Order?> GetByIdWithDetailsForUpdateAsync(int orderId);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
        Task<Order?> GetByOrderIdWithShipment(int orderId);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, OrderStatus.EnOrderStatus enOrderStatus);
        Task<IEnumerable<Order>> GetAllWithDetailsAsync();
        Task<IEnumerable<Order>> GetAllWithDetailsAsync(OrderStatus.EnOrderStatus enOrderStatus);
        Task<bool> HasCustomerPurchasedProductAsync(int customerId, int ProductId);
    }
}
