namespace Application.Features.Shipments.DTOs
{
    public class ShipmentDto
    {
        public int ShipmentId {  get; set; }
        public int OrderId {  get; set; }
        public byte ShipmentStatus {  get; set; }
        public string ShipmentStatusName { get; set; } = null!;
        public string? TrackingNumber {  get; set; }
        public string? CarrierName { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ShipmentOrderDto? Order { get; set; }
    }
}
