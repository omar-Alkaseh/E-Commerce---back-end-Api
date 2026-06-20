using Application.Features.Payments.DTOs;
using Contracts.Responses;
using Domain.Entities;

namespace ECommerce.Api.Mappings
{
    public static class PaymentContractMapping
    {
        public static PaymentResponse ToResponse(this PaymentDto payment)
        {
            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                FailedAt = payment.FailedAt,
                RefundedAt = payment.RefundedAt
            };
        }

        public static List<PaymentResponse> ToResponseList(this IEnumerable<PaymentDto> payments) =>
            payments.Select(p => p.ToResponse()).ToList();
    }
}