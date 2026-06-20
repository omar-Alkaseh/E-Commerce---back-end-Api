using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext context) : base(context) 
        {
            
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId) =>
            await _context.OrderItems
                .Where(o => o.OrderId == orderId)
                .Include(oi => oi.Order)
                .ToListAsync();
    }
}
