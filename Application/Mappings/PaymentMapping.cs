using Application.Features.Payments.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings
{
    public static class PaymentMapping
    {
        public static PaymentDto ToDto(this Payment payment)
        {
            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                PaymentMethod = ((PaymentMethod.EnPaymentMethod)payment.PaymentMethod).ToString(),
                PaymentStatus = ((PaymentStatus.EnPaymentStatus)payment.PaymentStatus).ToString(),
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                FailedAt = payment.FailedAt,
                RefundedAt = payment.RefundedAt
            };
        }

        public static List<PaymentDto> ToDtoList(this IEnumerable<Payment> payments) =>
            payments.Select(p => p.ToDto()).ToList();

    }
}
