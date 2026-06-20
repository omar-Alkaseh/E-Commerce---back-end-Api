namespace Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// 1 = Pending
    /// 2 = Processing
    /// 3 = Shipped
    /// 4 = Delivered
    /// 5 = Cancelled
    /// </summary>
    public byte OrderStatus { get; set; }

    public decimal TotalAmount { get; set; }

    public string ShippingAddressLine { get; set; } = null!;

    public string ShippingCity { get; set; } = null!;

    public string ShippingCountry { get; set; } = null!;

    public string? ShippingPostalCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Shipment? Shipment { get; set; }
}
