using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ShipmentRepository : BaseRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId) =>
            await _context.Shipments
                .AsNoTracking()
                .AnyAsync(x => x.OrderId == orderId);

        public async Task<IReadOnlyList<Shipment>> GetAllWithOrderAsync() =>
            await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Order)
                .ToListAsync();

        public async Task<Shipment?> GetByOrderIdAsync(int orderId) =>
            await _context.Shipments
                .FirstOrDefaultAsync(s => s.OrderId == orderId);

        public async Task<IEnumerable<Shipment>> GetMyShipmentAsync(int customerId) =>
            await _context.Shipments
                .Include(s => s.Order)
                .Where(s => s.Order.Customer.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<Shipment?> GetByShipmentIdWithOrderAndPaymentDetails(int shipmentId) =>
            await _context.Shipments
                .Include(s => s.Order)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);


        public async Task<IEnumerable<Shipment>> GetByShipmentsStatusWithOrderByDetails(ShippingStatus.EnShippingStatus status) =>
            await _context.Shipments
                .Include(s => s.Order)
                .Where(s => s.ShippingStatus == (byte)status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
                    
    }
}
