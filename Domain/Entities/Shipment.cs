namespace Domain.Entities;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    /// <summary>
    /// 1 = Pending
    /// 2 = Shipped
    /// 3 = InTransit
    /// 4 = Delivered
    /// 5 = Returned
    /// </summary>
    public byte ShippingStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public string? CarrierName { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public virtual Order Order { get; set; } = null!;
}
