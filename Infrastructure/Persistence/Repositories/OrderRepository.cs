using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync() =>
            await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Product)
                    .ThenInclude(o => o.ProductImages)
                .Include(o => o.Payments)
                .Include(o => o.Shipment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync(OrderStatus.EnOrderStatus enOrderStatus) =>
            await _context.Orders
                .AsNoTracking()
                .Where(o => (OrderStatus.EnOrderStatus)o.OrderStatus == enOrderStatus)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Product)
                    .ThenInclude(o => o.ProductImages)
                .Include(o => o.Payments)
                .Include(o => o.Shipment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId) =>
            await _context.Orders
                .Where(c => c.CustomerId == customerId)
                .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Product)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, OrderStatus.EnOrderStatus enOrderStatus) =>
            await _context.Orders
                .Where(c => c.CustomerId == customerId && (OrderStatus.EnOrderStatus)c.OrderStatus == enOrderStatus)
                .Include(o => o.OrderItems)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


        public async Task<Order?> GetByIdWithDetailsAsync(int orderId) =>
            await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

        public async Task<Order?> GetByIdWithDetailsForUpdateAsync(int orderId) =>
            await _context.Orders
                .Include(o => o.Shipment)
                .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

        public async Task<Order?> GetByOrderIdWithShipment(int orderId) =>
            await _context.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

        public async Task<bool> HasCustomerPurchasedProductAsync(int customerId, int ProductId) =>
            await _context.Orders
                .AnyAsync(
                o => o.CustomerId == customerId &&
                o.OrderStatus == (byte)OrderStatus.EnOrderStatus.Delivered &&
                o.OrderItems.Any(oi => oi.ProductId == ProductId));
    }
}
