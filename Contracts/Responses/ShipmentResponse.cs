namespace Contracts.Responses
{
    public class ShipmentResponse
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }

        public string Status { get; set; } = null!;
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }

        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ShipmentOrderResponse? Order { get; set; }
    }
}
