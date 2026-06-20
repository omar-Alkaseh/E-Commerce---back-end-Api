namespace Application.Features.Payments.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string PaymentStatus { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime? FailedAt { get; set; }

        public DateTime? RefundedAt { get; set; }
    }
}
