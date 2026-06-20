using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);

        Task<IEnumerable<Payment>> GetByOrderIdAndStatusAsync(
            int orderId,
            PaymentStatus.EnPaymentStatus status);

        Task<IEnumerable<Payment>> GetByOrderIdAndMethodAsync(
            int orderId,
            PaymentMethod.EnPaymentMethod method);

        Task<Payment?> GetLatestByOrderIdAsync(int orderId);

        Task<Payment?> GetByIdWithOrderAndShipment(int paymentId);

        Task<Payment?> GetByTransactionIdAsync(string transactionId);

        Task<bool> OrderHasPaidPaymentAsync(int orderId);
        Task<bool> HasActivePaymentForOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetMyPaymentsAsync(int customerId);
    }
}
