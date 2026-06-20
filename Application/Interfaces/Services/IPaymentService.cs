using Application.Features.Payments.DTOs;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();

        Task<IEnumerable<PaymentDto>> GetMyPaymentsAsync();

        Task<IEnumerable<PaymentDto>> GetByOrderIdAsync(int orderId);

        Task<PaymentDto> CreatePaymentAsync(int orderId, CreatePaymentDto paymentMethod);

        Task<PaymentDto> ProcessFakePaymentAsync(int paymentId, bool isSuccess, string? failureReason);
        
        Task<PaymentDto> RefundAsync(int paymentId);

    }
}
