using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId) =>
            await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Payment>> GetByOrderIdAndStatusAsync(int orderId, PaymentStatus.EnPaymentStatus enPaymentStatus) =>
            await _context.Payments
                .Where(p => p.OrderId == orderId && (PaymentStatus.EnPaymentStatus)p.PaymentStatus == enPaymentStatus)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId) =>
            await _context.Payments
                .SingleOrDefaultAsync(p => p.TransactionId == transactionId);

        public async Task<IEnumerable<Payment>> GetByOrderIdAndMethodAsync(int orderId, PaymentMethod.EnPaymentMethod enPaymentMethod) =>
            await _context.Payments
                .Where(p => p.OrderId == orderId && (PaymentMethod.EnPaymentMethod)p.PaymentMethod == enPaymentMethod)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<Payment?> GetLatestByOrderIdAsync(int orderId) =>
            await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();


        public async Task<bool> OrderHasPaidPaymentAsync(int orderId) =>
            await _context.Payments
                .AnyAsync(p => p.OrderId == orderId &&
                       (PaymentStatus.EnPaymentStatus)p.PaymentStatus == PaymentStatus.EnPaymentStatus.Paid);

        public async Task<bool> HasActivePaymentForOrderAsync(int orderId) =>
            await _context.Payments
                .AnyAsync(p => p.OrderId == orderId &&
                (
                    p.PaymentStatus == (byte)PaymentStatus.EnPaymentStatus.Paid     ||
                    p.PaymentStatus == (byte)PaymentStatus.EnPaymentStatus.Pending  ||
                    p.PaymentStatus == (byte)PaymentStatus.EnPaymentStatus.Refunded
                ));

        public async Task<IEnumerable<Payment>> GetMyPaymentsAsync(int customerId) =>
            await _context.Payments
                .Include(o => o.Order)
                .Where(o => o.Order.CustomerId == customerId)
                .ToListAsync();

        public async Task<Payment?> GetByIdWithOrderAndShipment(int paymentId) =>
            await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Shipment)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }
}
