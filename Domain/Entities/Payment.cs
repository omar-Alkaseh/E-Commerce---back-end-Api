namespace Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    /// <summary>
    /// 1 = CashOnDelivery
    /// 2 = Card
    /// 3 = PayPal
    /// </summary>
    public byte PaymentMethod { get; set; }

    /// <summary>
    /// 1 = Pending
    /// 2 = Paid
    /// 3 = Failed
    /// 4 = Refunded
    /// </summary>
    public byte PaymentStatus { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
